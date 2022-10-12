using System;
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
using Proxy.Core.Commands;
using Proxy.Core.Services;
using Proxy.DiscordProxy;
using Proxy.ESP.Api;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace Proxy.Command;

public class SearchFunction
{
  private readonly JsonService _jsonService;
  private readonly EnvironmentService _envService;
  private readonly DiscordHandler _discordClient;
  private readonly IEskomSePushClient _espClient;

  private readonly IInvoker _invoker;
  private readonly ILogger<SearchFunction> _logger;

  public SearchFunction()
  {
    Console.WriteLine("SearchFunction.ctor");

    Shell.ConfigureServices(collection =>
    {
      collection.AddSingleton<DiscordHandler>();

      collection.AddSingleton<IInvoker, Invoker>();
      collection.AddSingleton<IEskomSePushClient, EskomSePushClient>();

      // AWS
      var endpoint = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION"));
      collection.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(endpoint));

      // Repo
      // collection.AddSingleton<UserSeriesRepository>();
      // var dbDetails = Api.LoadDatabaseSecret();
      // collection.AddSingleton(new UserRepository(dbDetails.ToString()));
    });

    _jsonService = Shell.Get<JsonService>();
    _envService = Shell.Get<EnvironmentService>();
    _discordClient = Shell.Get<DiscordHandler>();
    _espClient = Shell.Get<IEskomSePushClient>();

    _invoker = Shell.Get<IInvoker>();
    _logger = Shell.Get<ILogger<SearchFunction>>();
  }

  public async Task FunctionHandler(SNSEvent request, ILambdaContext context)
  {
    _logger.LogInformation("Attempting SearchFunction.function");
    _logger.LogDebug(_jsonService.Serialize(request));

    // Validate SNS Record
    var snsRecord = request.Records?.FirstOrDefault();

    if (snsRecord is null)
    {
      _logger.LogCritical("Failed to get SNS Record");
      return;
    }

    if (request.Records.Count > 1)
    {
      _logger.LogWarning($"SNS received with {request.Records.Count} records");
    }

    var messageHealthy = _jsonService.TryDeserialize<DiscordInteraction>(snsRecord.Sns.Message, out var interaction);
    if (!messageHealthy)
    {
      _logger.LogCritical("SNS Message is unhealthy");
      return;
    }

    // create response with info
    var searchText = interaction.Data.Options[0].Value.ToString()!;
    var user = interaction.Member.User;
    var espAuthToken = _envService.Get("ESP_AUTH_TOKEN");
    var s3AssetBucket = _envService.Get("S3_ASSET_BUCKET");

    _logger.LogDebug("~~~~~");
    _logger.LogDebug($"searchText: {searchText}");
    _logger.LogDebug($"searchText: {_jsonService.Serialize(user)}");
    _logger.LogDebug($"searchText: {espAuthToken}");
    _logger.LogDebug($"searchText: {s3AssetBucket}");
    _logger.LogDebug("~~~~~");

    // get ESP data
    var searchResults = await _espClient.SearchByText(searchText);

    var embed = new EmbedBuilder()
      .WithTitle($"Results for '{searchText}'")
      .WithDescription($"{searchResults.Areas.Count} results")
      .AddField("Field title", "Field value. I also support [hyperlink markdown](https://example.com)!")
      // .WithAuthor(Context.Client.CurrentUser)
      // .WithFooter(footer =>
      // {
      //   footer.Text = "I am a footer.";
      // })
      .WithColor(Color.Blue);
      // .WithTitle("I overwrote \"Hello world!\"")
      // .WithDescription("I am a description.")
      // .WithUrl("https://example.com")

    foreach (var (id, name, region) in searchResults.Areas)
    {
      embed.AddField("id", name, inline: true);
      embed.AddField("name", id, inline: true);
      embed.AddField("region", region, inline: true);
    }

    await _discordClient.Handle(interaction, embed);
  }
}
