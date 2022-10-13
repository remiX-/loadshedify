﻿using System;
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
using Proxy.Core;
using Proxy.Core.Services;
using Proxy.DiscordProxy;
using Proxy.ESP.Api;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace Proxy.Command;

public class SearchFunction
{
  private readonly JsonService _jsonService;
  private readonly DiscordHandler _discordClient;
  private readonly IEskomSePushClient _espClient;

  private readonly ILogger<SearchFunction> _logger;

  public SearchFunction()
  {
    Console.WriteLine("SearchFunction.ctor");

    Shell.ConfigureServices(collection =>
    {
      collection.AddSingleton<DiscordHandler>();
      collection.AddSingleton<IEskomSePushClient, EskomSePushClient>();

      // AWS
      // TODO 
      var endpoint = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION"));
      collection.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(endpoint));
    });

    _jsonService = Shell.Get<JsonService>();
    _discordClient = Shell.Get<DiscordHandler>();
    _espClient = Shell.Get<IEskomSePushClient>();

    _logger = Shell.Get<ILogger<SearchFunction>>();
  }

  public async Task FunctionHandler(SNSEvent request, ILambdaContext context)
  {
    if (!Validate(request, out var interaction)) return;

    var searchText = interaction.Data.Options[0].Value.ToString()!.Trim();
    var searchResults = await _espClient.SearchByText(searchText);

    var embed = new EmbedBuilder()
      .WithTitle($"Results for '{searchText}'")
      .WithDescription($"{searchResults.Areas.Count} results")
      .WithColor(Color.Blue);

    // TODO reduce number of fields
    foreach (var (id, name, region) in searchResults.Areas)
    {
      embed.AddField("id", name, inline: true);
      embed.AddField("name", id, inline: true);
      embed.AddField("region", region, inline: true);
    }

    await _discordClient.Handle(interaction, embed);

    _logger.LogInformation("Great success!");
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
