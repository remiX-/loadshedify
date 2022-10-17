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

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace Proxy.Command;

public class SearchFunction
{
  private readonly IJsonService _jsonService;
  private readonly CommandHandler _commandHandler;
  private readonly IEskomSePushClient _espClient;

  private readonly ILogger<SearchFunction> _logger;

  public SearchFunction()
  {
    Console.WriteLine("SearchFunction.ctor");

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

    _logger = Shell.Get<ILogger<SearchFunction>>();
  }

  public async Task FunctionHandler(SNSEvent request, ILambdaContext context)
  {
    await _commandHandler.Handle(request, Action);
  }

  private async Task<IReadOnlyList<EmbedBuilder>> Action(DiscordInteraction interaction)
  {
    var searchText = interaction.Data.Options[0].Value.ToString()!.Trim();
    var searchResults = await _espClient.SearchByText(searchText);

    // embed per region
    var distinctRegions = searchResults.Areas.Select(area => area.Region).Distinct();
    var embeds = new List<EmbedBuilder>();

    foreach (var distinctRegion in distinctRegions)
    {
      var regionAreas = searchResults.Areas.Where(area => area.Region.Equals(distinctRegion)).ToArray();

      var embed = new EmbedBuilder()
        .WithTitle(distinctRegion)
        .WithDescription($"{regionAreas.Length} result{(regionAreas.Length == 1 ? "" : "s")}")
        .WithColor(Color.Blue);

      foreach (var (id, name, _) in regionAreas)
      {
        embed.AddField("Name", name, true);
        embed.AddField("Id", id, true);
        embed.AddInlineEmptyField();
      }

      embeds.Add(embed);
    }

    return embeds;
  }
}
