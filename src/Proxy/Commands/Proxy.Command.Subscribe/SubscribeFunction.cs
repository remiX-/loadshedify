using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Channels;
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
using Proxy.ESP.Api;
using Proxy.ESP.Api.Entity;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace Proxy.Command;

public class SubscribeFunction
{
  private readonly IJsonService _jsonService;
  private readonly CommandHandler _commandHandler;
  private readonly IEskomSePushClient _espClient;
  private readonly SubRepository _subRepo;

  private readonly ILogger<SubscribeFunction> _logger;

  public SubscribeFunction()
  {
    Shell.ConfigureServices(collection =>
    {
      collection.AddSingleton<CommandHandler>();
      collection.AddSingleton<DiscordHandler>();
      collection.AddSingleton<IEskomSePushClient, EskomSePushClient>();

      // AWS
      var endpoint = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION"));
      collection.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(endpoint));
      collection.AddSingleton<IDynamoService, DynamoService>();

      collection.AddSingleton<SubRepository>();
    });

    _commandHandler = Shell.Get<CommandHandler>();
    _subRepo = Shell.Get<SubRepository>();

    _logger = Shell.Get<ILogger<SubscribeFunction>>();
  }

  public async Task FunctionHandler(SNSEvent request, ILambdaContext context)
  {
    await _commandHandler.Handle(request, Action);
  }

  private async Task<IReadOnlyList<EmbedBuilder>> Action(DiscordInteraction interaction)
  {
    // var userId = interaction.Member.User.Id;
    // var channelId = interaction.ChannelId;
    var subCommand = interaction.Data.Options.First();
    var areaId = subCommand.Options.First().Value.ToString()!.Trim();

    var embed = new EmbedBuilder();
    if (subCommand.Name.Equals("add"))
    {
      await AddSub(interaction, embed, areaId);
    }
    else
    {
      embed
        .WithTitle("Can't remove yet sorry :(");
    }

    return new List<EmbedBuilder> { embed };
  }

  private async Task AddSub(DiscordInteraction interaction, EmbedBuilder embed, string areaId)
  {
    var userId = interaction.Member.User.Id;
    var channelId = interaction.ChannelId;

    var success = await _subRepo.Add(userId, channelId, areaId);

    if (success)
    {
      var simMatch = Regex.Match(areaId, @"sim\.(-?\d+)m_(.+)");

      embed
        .WithTitle("Subscription added")
        .WithDescription(
          $"**Area:** {(simMatch.Success ? simMatch.Groups[2] : areaId)}\n" +
          $"**Type:** {(simMatch.Success ? "Simulated" : "Standard")}"
        )
        .WithColor(simMatch.Success ? Color.DarkTeal : Color.Green);
    }
    else
    {
      embed
        .WithTitle("You're already subscribed, dummy :)")
        .WithColor(Color.Red);
    }
  }

}
