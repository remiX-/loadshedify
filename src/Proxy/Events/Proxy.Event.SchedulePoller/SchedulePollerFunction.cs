using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proxy.Core;
using Proxy.Core.DataModels.Dynamo;
using Proxy.Core.Model;
using Proxy.Core.Services;
using Proxy.DiscordProxy;
using Proxy.ESP.Api;
using Proxy.ESP.Api.Entity;
using Proxy.ESP.Api.Extensions;
using Proxy.ESP.Api.Response;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace Proxy.Event;

public class SchedulePollerFunction
{
  private readonly IEskomSePushClient _espClient;
  private readonly IJsonService _jsonService;
  private readonly IDynamoService _dbService;
  private readonly DiscordHandler _discordHandler;

  private readonly ILogger<SchedulePollerFunction> _logger;

  private const int DefaultAlertThresholdMinutes = 24 * 60;
  private readonly int _alertThresholdMinutes;

  private DateTimeOffset _executionStartTime;

  public SchedulePollerFunction()
  {
    Shell.ConfigureServices(collection =>
    {
      collection
        .WithEspClient()
        .WithDiscordHandler()
        .WithDynamoDb();
    });

    _espClient = Shell.Get<IEskomSePushClient>();
    _jsonService = Shell.Get<IJsonService>();
    _dbService = Shell.Get<IDynamoService>();
    _discordHandler = Shell.Get<DiscordHandler>();

    _logger = Shell.Get<ILogger<SchedulePollerFunction>>();

    if (_alertThresholdMinutes == 0)
    {
      var envModel = Shell.Get<IEnvironmentModel>();
      _alertThresholdMinutes = envModel.GetInt("ALERT_THRESHOLD", DefaultAlertThresholdMinutes);
      _logger.LogDebug($"Setting AlertThreshold: {_alertThresholdMinutes}");
    }
  }

  public async Task FunctionHandler(object request, ILambdaContext context)
  {
    _logger.LogDebug($"Subscriptions table: {TableNames.Sub}");

    // Setup
    _executionStartTime = DateTimeOffset.UtcNow;

    // Read for DynamoDB for subscriptions
    var userIdToSubsMap = await _dbService.ScanTable(TableNames.Sub);

    // Filter out "test" users that I kept for testing
    userIdToSubsMap = userIdToSubsMap.Where(userSub =>
    {
      var userId = userSub[TableNameColumns.UserId].S;
      return !userId.Contains("_");
    }).ToList();

    if (!userIdToSubsMap.Any())
    {
      _logger.LogDebug("No active subscriptions");
      return;
    }

    // Get messages per channel per area id for list of users
    GetMessageCombinations(userIdToSubsMap, out var channelAreaIdToUserIdMap, out var uniqueAreaIds);
    _logger.LogDebug(_jsonService.Serialize(uniqueAreaIds, true));

    // Get scheduleInfo for unique area ids
    var getScheduleTasks = uniqueAreaIds.Select(GetAreaSchedule);
    var schedules = await Task.WhenAll(getScheduleTasks);

    // Send the messages on Discord
    var sendMessageTasks = channelAreaIdToUserIdMap
      .Select(kvp => SendMessage(kvp.Key, kvp.Value, schedules))
      .ToList();
    _logger.LogDebug($"Processing {sendMessageTasks.Count} message combinations");
    await Task.WhenAll(sendMessageTasks);

  }

  private void GetMessageCombinations(IList<DynamoItem> userIdToSubsMap, out Dictionary<string, IList<string>> channelAreaIdToUserIdMap, out IEnumerable<string> uniqueAreaIds)
  {
    channelAreaIdToUserIdMap = new Dictionary<string, IList<string>>();
    uniqueAreaIds = new List<string>();

    foreach (var userSub in userIdToSubsMap)
    {
      var userId = userSub[TableNameColumns.UserId].S;
      var channelMappings = userSub[TableNameColumns.SubMappings].M;

      foreach (var (channelId, channelMapping) in channelMappings)
      {
        uniqueAreaIds = uniqueAreaIds.Concat(channelMapping.SS).Distinct();

        foreach (var s in channelMapping.SS)
        {
          var mapKey = $"{channelId}:{s}";
          if (!channelAreaIdToUserIdMap.ContainsKey(mapKey)) channelAreaIdToUserIdMap.Add(mapKey, new List<string>());
          channelAreaIdToUserIdMap[$"{channelId}:{s}"].Add(userId);
        }
      }
    }
  }

  private async Task<(string areaId, AreaScheduleResponse response)> GetAreaSchedule(string areaId)
  {
    // Check if areaId is a "SIM" area for demo purposes
    var simMatch = Regex.Match(areaId, @"sim\.(-?\d+)m_(.+)");
    if (simMatch.Success) return GetSimulatedSchedule(simMatch);

    var result = await _espClient.GetAreaSchedule(areaId);

    return (areaId, result);
  }

  /*
   sim examples:
   sim.60m_my-simulated-area-id
   sim.-10m_simulate-active
   */
  private (string areaId, AreaScheduleResponse response) GetSimulatedSchedule(Match simMatch)
  {
    var simDuration = int.Parse(simMatch.Groups[1].Value);
    var simAreaId = simMatch.Groups[2].Value;
    var saNow = _executionStartTime
      .ToOffset(TimeSpan.FromHours(2))
      .AddSeconds(_executionStartTime.Second * -1);

    var simSchedule = new AreaScheduleResponse
    {
      // Add an event that's happening in a random time
      Events = new List<AreaScheduleEvent>
      {
        new()
        {
          Note = "Stage 5",
          Start = saNow.AddMinutes(simDuration + 1), // Add additional minute for rounding
          End = saNow.AddMinutes(simDuration + 1).AddHours(2)
        }
      }
    };

    return (simAreaId, simSchedule);
  }

  private async Task SendMessage(string channelIdAreaId, IList<string> userList, (string areaId, AreaScheduleResponse response)[] schedules)
  {
    // just wrap everything in try catch for bad input
    // ofc would need to be fixed and inputs validated :)
    try
    {
      var split = channelIdAreaId.Split(":");
      var channelId = split[0];
      var areaId = split[1].Contains("_") ? split[1].Split("_")[1] : split[1];
      var areaSchedule = schedules.First(schedule => schedule.areaId == areaId).response;

      // Check if schedule has any, otherwise don't continue
      if (!areaSchedule.Events.Any())
      {
        _logger.LogDebug($"No events for area: {areaId}");
        return;
      }

      // get first event - assuming it's sorted from API? ;P
      var scheduleEvent = areaSchedule.Events.First();

      var embed = new EmbedBuilder();

      if (scheduleEvent.HasStarted)
      {
        embed
          .WithTitle($"ALERT: Active Loadshedding for {areaId}")
          .WithDescription(
            $"**Status:**: Stage {scheduleEvent.Stage} ends in {scheduleEvent.PrettyTimeToEnd(_executionStartTime)}\n" +
            $"**Start:** {scheduleEvent.Start:dd/MM/yyyy HH:mm}\n" +
            $"**End:** {scheduleEvent.End:dd/MM/yyyy HH:mm}"
          )
          .WithColor(Color.Red);
      }
      else
      {
        embed
          .WithTitle($"ALERT: Upcoming Loadshedding for {areaId}")
          .WithDescription(
            $"**Status:**: Stage {scheduleEvent.Stage} starts in {scheduleEvent.PrettyTimeToStart(_executionStartTime)}\n" +
            $"**Start:** {scheduleEvent.Start:dd/MM/yyyy HH:mm}\n" +
            $"**End:** {scheduleEvent.End:dd/MM/yyyy HH:mm}"
          )
          .WithColor(Color.Gold);
      }

      var messageContent = $"{userList.Select(userId => $"<@{userId}>").Aggregate((c, n) => $"{c} {n}")}";
      await _discordHandler.SendMessage(channelId, messageContent, embed);
    }
    catch (Exception ex)
    {
      // Just ignore any failed input
      _logger.LogDebug($"SendMessage failed: {ex.Message}");
    }
  }
}
