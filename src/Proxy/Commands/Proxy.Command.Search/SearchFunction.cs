using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
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
using Proxy.ESP.Api.Entity;
using Proxy.ESP.Api.Response;

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
      collection.AddSingleton<CommandHandler>();
      collection.AddSingleton<DiscordHandler>();
      collection.AddSingleton<IEskomSePushClient, EskomSePushClient>();

      // AWS
      // TODO 
      var endpoint = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION"));
      collection.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(endpoint));
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

// #if DEBUG
//     var searchResults = await GetFakeSearchResponse();
// #endif

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

#if DEBUG
  private Task<SearchTextResponse> GetFakeSearchResponse()
  {
    return Task.FromResult(new SearchTextResponse()
    {
      Areas = new List<Area>
      {
        new() { Id = "ethekwini2-4-umhlangaeast", Name = "Umhlanga East (4)", Region = "eThekwini Municipality" },
        new() { Id = "ethekwini2-9-umhlanganoot", Name = "Umhlanga Noot (9)", Region = "eThekwini Municipality" },
        new() { Id = "ethekwini2-12-umhlangasouth", Name = "Umhlanga South (12)", Region = "Not eThekwini Municipality" },
        new() { Id = "very-1-real", Name = "Very Real (1)", Region = "Fake Region" },
        new() { Id = "hello-5-lol", Name = "Hello (5)", Region = "Lol:)" }
      }
    });
  }
#endif
}
