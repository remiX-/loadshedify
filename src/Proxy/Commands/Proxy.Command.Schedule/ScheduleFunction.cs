using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SNSEvents;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proxy.Core;
using Proxy.Core.Services;
using Proxy.DiscordProxy;
using Proxy.ESP.Api;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace Proxy.Command;

public class ScheduleFunction
{
  private readonly JsonService _jsonService;
  private readonly DiscordHandler _discordClient;
  private readonly IEskomSePushClient _espClient;

  private readonly ILogger<ScheduleFunction> _logger;

  public ScheduleFunction()
  {
    Console.WriteLine("SearchFunction.ctor");

    Shell.ConfigureServices(collection =>
    {
      collection.AddSingleton<DiscordHandler>();
      collection.AddSingleton<IEskomSePushClient, EskomSePushClient>();
    });

    _jsonService = Shell.Get<JsonService>();
    _discordClient = Shell.Get<DiscordHandler>();
    _espClient = Shell.Get<IEskomSePushClient>();

    _logger = Shell.Get<ILogger<ScheduleFunction>>();
  }

  public async Task FunctionHandler(SNSEvent request, ILambdaContext context)
  {
    if (!Validate(request, out var interaction)) return;

    var areaId = interaction.Data.Options[0].Value.ToString()!.Trim();
    var day = GetDay(interaction.Data.Options);
    day = "thursday";
    var searchResults = await _espClient.GetAreaSchedule(areaId);
    var schedule = searchResults.Schedule.Days.First(d => d.Name.Equals(day, StringComparison.OrdinalIgnoreCase));

    var embed = new EmbedBuilder()
      .WithTitle($"Stage schedule for ${areaId}")
      .WithDescription($"Date: ${schedule.Name}, ${schedule.Date}\nRegion: ${searchResults.Info.Region}")
      .WithColor(Color.DarkOrange);

    for (int stageIndex = 0; stageIndex < schedule.Stages.Count; stageIndex++)
    {
      var stage = schedule.Stages[stageIndex];
      var stageHasSlots = stage.Count > 0;
      var val = "None";

      if (stageHasSlots)
      {
        val = stage.Aggregate((first, next) => $"{first}\n{next}").Replace("-", " - ");
      }

      embed.AddField($"Stage {stageIndex + 1}", val, inline: true);

      // Add blank every 2 stages to force 2 columns
      if (stageIndex % 2 == 0) embed.AddField("\u200B", "\u200B", inline: true);
    }

    // TODO reduce number of fields
    // foreach (var (id, name, region) in searchResults.Areas)
    // {
    //   embed.AddField("id", name, inline: true);
    //   embed.AddField("name", id, inline: true);
    //   embed.AddField("region", region, inline: true);
    // }

    await _discordClient.Handle(interaction, embed);

    _logger.LogInformation("Great success!");
  }

  private string GetDay(IList<DiscordDataOption> options)
  {
    var today = DateTime.Now.DayOfWeek.ToString();
    if (options.Count < 2)
    {
      // no day specified
      _logger.LogDebug($"No day specified, using today {today}");
    }

    return today;
  }

  private bool Validate(SNSEvent request, out DiscordInteraction interaction)
  {
    interaction = default;

    _logger.LogDebug(_jsonService.Serialize(request));

    // Validate SNS Record
    var snsRecord = request.Records?.FirstOrDefault();

    if (snsRecord is null)
    {
      _logger.LogCritical("Failed to get SNS Record");
      return false;
    }

    if (request.Records.Count > 1)
    {
      _logger.LogWarning($"SNS received with {request.Records.Count} records");
    }

    var messageHealthy = _jsonService.TryDeserialize<DiscordInteraction>(snsRecord.Sns.Message, out interaction);
    if (!messageHealthy)
    {
      _logger.LogCritical("SNS Message is unhealthy");
      return false;
    }

    return true;
  }
}
