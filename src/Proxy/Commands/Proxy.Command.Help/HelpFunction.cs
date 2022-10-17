using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
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
using Proxy.DiscordProxy.Builders;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace Proxy.Command;

public class HelpFunction
{
  private readonly CommandHandler _commandHandler;

  private readonly ILogger<HelpFunction> _logger;

  public HelpFunction()
  {
    Console.WriteLine("StatusFunction.ctor");

    Shell.ConfigureServices(collection =>
    {
      collection.AddSingleton<CommandHandler>();
    });

    _commandHandler = Shell.Get<CommandHandler>();

    _logger = Shell.Get<ILogger<HelpFunction>>();
  }

  public async Task FunctionHandler(SNSEvent request, ILambdaContext context)
  {
    await _commandHandler.Handle(request, Action);
  }

  private async Task<IReadOnlyList<EmbedBuilder>> Action(DiscordInteraction interaction)
  {

    var embeds = new List<EmbedBuilder>();

    foreach (var (_, status) in statusResponse.Status)
    {
      var stage = int.Parse(status.Stage);
      var embed = new EmbedBuilder()
        .WithTitle(status.Name)
        .WithDescription(
          stage == 0
            ? "**No loadshedding!**"
            : $"**Current stage:** {status.Stage}\n**Updated at:** {status.Updated}"
        )
        .WithColor(GetColorForStage(stage));
    
      embeds.Add(embed);
    
      if (status.NextStages.Count == 0) continue;
    
      embed.AddField("Upcoming events below", "Oh noes!");
    
      foreach (var next in status.NextStages)
      {
        var nextStage = int.Parse(next.Stage);
    
        // var easternZone = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");
        // var today = TimeZoneInfo.ConvertTimeFromUtc(next.Timestamp, easternZone);
        // embed.AddField($"Stage {nextStage}", next.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
        embed.AddField($"Stage {nextStage}", next.Timestamp.ToString("dd/MM/yyyy HH:mm"));
      }
    }

    return embeds;
  }

  // private Color GetColorForStage(int stage)
  // {
  //   return stage switch
  //   {
  //     0 => Color.Green,
  //     1 or 2 => new Color(248, 255, 18),
  //     3 or 4 => Color.LightOrange,
  //     5 => Color.DarkOrange,
  //     6 or 7 or 8 => Color.Red,
  //     _ => throw new IndexOutOfRangeException($"Stage out of bounds of 1 - 8: {stage}")
  //   };
  // }
}
