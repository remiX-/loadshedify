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
using Proxy.Command.Handler;
using Proxy.Core;
using Proxy.Core.Services;
using Proxy.DiscordProxy;
using Proxy.DiscordProxy.Extensions;
using Proxy.ESP.Api;

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
    Console.WriteLine("SearchFunction.ctor");

    Shell.ConfigureServices(collection =>
    {
      collection.AddSingleton<CommandHandler>();
      collection.AddSingleton<DiscordHandler>();
      collection.AddSingleton<IEskomSePushClient, EskomSePushClient>();
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
    var areaId = interaction.Data.Options[0].Value.ToString()!.Trim();
    var day = GetDay(interaction.Data.Options.FirstOrDefault(option => option.Name.Equals("day")));
    var searchResults = await _espClient.GetAreaSchedule(areaId);

    if (searchResults.Events is null)
    {
      throw new Exception($"Invalid area id: '{areaId}'");
    }

    var schedule = searchResults.Schedule.Days.First(d => d.Name.Equals(day, StringComparison.OrdinalIgnoreCase));

    var embed = new EmbedBuilder()
      .WithTitle($"Stage schedule for {areaId}")
      .WithDescription($"**Date:** {schedule.Name}, {schedule.Date}\n**Region:** {searchResults.Info.Region}")
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

      embed.AddField($"Stage {stageIndex + 1}", val, true);

      // Add blank every 2 stages to force 2 columns
      if (stageIndex % 2 == 0) embed.AddInlineEmptyField();
    }

    return new List<EmbedBuilder> { embed };
  }

  private string GetDay(DiscordDataOption? option)
  {
    var today = DateTime.Now.DayOfWeek.ToString();

    if (!option.HasValue)
    {
      // no day specified
      _logger.LogDebug($"No day specified, using today: {today}");
      return today;
    }

    var day = option.Value.Value.ToString();
    var valid = Enum.TryParse<DayOfWeek>(day, true, out var specifiedDay);
    if (valid) return specifiedDay.ToString();

    _logger.LogDebug($"Specified day '{day}' is not valid, using today: {today}");
    return today;
  }
}
