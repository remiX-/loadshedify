using System;
using System.Collections.Generic;
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

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace Proxy.Command;

public class SubscribeFunction
{
  private readonly IJsonService _jsonService;
  private readonly CommandHandler _commandHandler;
  private readonly IEskomSePushClient _espClient;

  private readonly ILogger<SubscribeFunction> _logger;

  public SubscribeFunction()
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

    _logger = Shell.Get<ILogger<SubscribeFunction>>();
  }

  public async Task FunctionHandler(SNSEvent request, ILambdaContext context)
  {
    await _commandHandler.Handle(request, Action);
  }

  private async Task<IReadOnlyList<EmbedBuilder>> Action(DiscordInteraction interaction)
  {
    var embed = new EmbedBuilder()
      .WithTitle("COMING SOON!")
      .WithDescription("Watch this space...")
      .WithColor(Color.DarkPurple);

    return new List<EmbedBuilder> { embed };
  }
}
