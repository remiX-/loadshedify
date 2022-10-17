using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SNSEvents;
using Discord;
using Microsoft.Extensions.Logging;
using Proxy.Command.Handler;
using Proxy.Core;
using Proxy.Core.Services;
using Proxy.DiscordProxy;
using Proxy.DiscordProxy.Extensions;
using Proxy.ESP.Api;
using Proxy.ESP.Api.Entity;
using Proxy.ESP.Api.Extensions;
using Proxy.ESP.Api.Response;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace Proxy.Command;

public class ScheduleFunction
{
  private readonly IJsonService _jsonService;
  private readonly CommandHandler _commandHandler;
  private readonly IEskomSePushClient _espClient;

  private readonly ILogger<ScheduleFunction> _logger;

  public ScheduleFunction()
  {
    Shell.ConfigureServices(collection =>
    {
      collection
        .WithCommandProxy()
        .WithEspClient()
        .WithDiscordHandler();
    });

    _jsonService = Shell.Get<IJsonService>();
    _commandHandler = Shell.Get<CommandHandler>();
    _espClient = Shell.Get<IEskomSePushClient>();

    _logger = Shell.Get<ILogger<ScheduleFunction>>();
  }

  public async Task FunctionHandler(SNSEvent request, ILambdaContext context)
  {
    await _commandHandler.Handle(request, Action);
  }

  private async Task<IReadOnlyList<EmbedBuilder>> Action(DiscordInteraction interaction)
  {
    var commandName = interaction.Data.Name;
    var (areaId, scheduleResponse) = await GetSchedule(commandName, interaction.Data.Options.First());

    if (scheduleResponse.Events is null)
    {
      throw new Exception($"Invalid area id: '{areaId}'");
    }

    var day = GetDay(interaction.Data.Options.FirstOrDefault(option => option.Name.Equals("day")));
    var schedule = scheduleResponse.Schedule.Days.First(d => d.Name.Equals(day, StringComparison.OrdinalIgnoreCase));

    var embed = new EmbedBuilder()
      .WithTitle($"Stage schedule for {areaId}")
      .WithDescription($"**Date:** {schedule.Name}, {schedule.Date}\n**Region:** {scheduleResponse.Info.Region}")
      .WithColor(Color.Magenta);

    if (scheduleResponse.Events.Count > 0)
    {
      // Loadshedding happening now or in future
      AddEvents(embed, scheduleResponse.Events);
    }

    for (int stageIndex = 0; stageIndex < schedule.Stages.Count; stageIndex++)
    {
      var stage = schedule.Stages[stageIndex];
      var stageHasSlots = stage.Count > 0;
      var val = "None";

      if (stageHasSlots)
      {
        val = stage.Aggregate((first, next) => $"{first}\n{next}").Replace("-", " - ");
      }

      embed.AddField($"Stage {stageIndex + 1}", val, true);

      // Add blank every 2 stages to force 2 columns
      if (stageIndex % 2 == 0) embed.AddInlineEmptyField();
    }

    return new List<EmbedBuilder> { embed };
  }

  private async Task<(string areaId, AreaScheduleResponse response)> GetSchedule(string commandName, DiscordDataOption option)
  {
    string testSim = null;
    string areaId;

    if (commandName.Equals("schedule"))
    {
      areaId = option.Value.ToString()!.Trim();
    }
    else
    {
      testSim = option.Name;
      areaId = option.Options[0].Value.ToString()!.Trim();
    }

    var response = await _espClient.GetAreaSchedule(areaId, testSim);

    return (areaId, response);
  }

  private void AddEvents(EmbedBuilder embed, IList<AreaScheduleEvent> events)
  {
    foreach (var ev in events)
    {
      if (ev.HasStarted) AddCurrentEvent(embed, ev);
      else AddFutureEvent(embed, ev);
    }
  }

  private void AddCurrentEvent(EmbedBuilder embed, AreaScheduleEvent ev)
  {
    embed.Description += $"\n**Status:** :( Stage {ev.Stage}";
    embed.AddEmptyFieldWithName($"ACTIVE Stage {ev.Stage}, ends in {ev.PrettyTimeToEnd(DateTimeOffset.UtcNow)}");
    embed.WithColor(Color.Red);
  }

  private void AddFutureEvent(EmbedBuilder embed, AreaScheduleEvent ev)
  {
    embed.Description += "\n**Status:** All good, woot! For now... :/";
    embed.AddEmptyFieldWithName($"UPCOMING Stage {ev.Stage}, starts in {ev.PrettyTimeToStart(DateTimeOffset.UtcNow)}");
    embed.WithColor(Color.Orange);
  }

  private string GetDay(DiscordDataOption option)
  {
    var today = DateTime.Now.DayOfWeek.ToString();

    if (option.Name is null)
    {
      // no day specified
      _logger.LogDebug($"No day specified, using today: {today}");
      return today;
    }

    var day = option.Value.ToString();
    var valid = Enum.TryParse<DayOfWeek>(day, true, out var specifiedDay);
    if (valid) return specifiedDay.ToString();

    _logger.LogDebug($"Specified day '{day}' is not valid, using today: {today}");
    return today;
  }
}
