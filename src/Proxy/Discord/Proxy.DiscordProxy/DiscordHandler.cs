using Discord;
using Microsoft.Extensions.Logging;
using Proxy.Core.DataModels.Web;
using Proxy.Core.Model;
using Proxy.Core.Services;

namespace Proxy.DiscordProxy;

public class DiscordHandler
{
  private readonly IHttpService _httpService;
  private readonly IJsonService _jsonService;
  private readonly IVariablesModel _varModel;
  private readonly ILogger<DiscordHandler> _logger;

  public DiscordHandler(IHttpService httpService,
    IJsonService jsonService,
    IVariablesModel varModel,
    ILogger<DiscordHandler> logger)
  {
    _httpService = httpService;
    _jsonService = jsonService;
    _varModel = varModel;
    _logger = logger;
  }

  public async Task HandleCallback(string id, string token)
  {
    var httpRequest = _httpService.NewRequest();

    // if (interaction.Dev)
    // {
    //   httpRequest
    //     .WithMethod(HttpMethods.Post)
    //     .WithUrl("https://discord.com/api/v10/interactions", "<interaction_id>", "<interaction_token>", "callback");
    // }
    // else
    // {
    httpRequest
      .WithMethod(HttpMethods.Patch)
      .WithUrl("https://discord.com/api/v10/interactions", id, token, "callback")
      .WithBody(new
      {
        type = 4,
        data = new
        {
          content = "Loading..."
        }
      });

    var result = await _httpService.ExecuteAsync<object>(httpRequest.Build());
    _logger.LogDebug(_jsonService.Serialize(result));

    _logger.LogInformation($"Message callback updated: {result.Result}");
  }

  public async Task Handle(DiscordInteraction interaction, EmbedBuilder embed)
  {
    await Handle(interaction, new List<EmbedBuilder> { embed });
  }

  public async Task Handle(DiscordInteraction interaction, IReadOnlyList<EmbedBuilder> embeds)
  {
    // Add defaults to embed
    foreach (var embed in embeds)
    {
      embed
        .WithCurrentTimestamp()
        .WithFooter(footer =>
        {
          footer.Text = "Developed by remiX";
          // TODO convert to S3 Service
          footer.IconUrl = $"https://{_varModel.S3AssetBucket}.s3.eu-west-1.amazonaws.com/assets/images/electricity.png";
        });
    }

    var httpRequest = _httpService.NewRequest();

    if (interaction.Dev)
    {
      httpRequest
        .WithMethod(HttpMethods.Post)
        .WithUrl("https://discord.com/api/webhooks/1029440747482644631/IqUMAzp5APKnzPIL3URskC8KYkvLrgscvdHUYDxPa8VJBdagFqzJTYIpav4C3OiMN3ll");
    }
    else
    {
      httpRequest
        .WithMethod(HttpMethods.Patch)
        .WithUrl("https://discord.com/api/v10/webhooks", interaction.ApplicationId, interaction.Token, "messages/@original");
    }

    httpRequest.WithBody(new
    {
      content = $"<@{interaction.Member.User.Id}>",
      embeds = embeds.Select(embed => new DiscordEmbed(embed.Build()))
    });

    var result = await _httpService.ExecuteAsync<object>(httpRequest.Build());

    _logger.LogInformation($"Message updated: {result.Result}");
  }

  public async Task HandleError(DiscordInteraction interaction, string error)
  {
    var httpRequest = _httpService.NewRequest();

    if (interaction.Dev)
    {
      httpRequest
        .WithMethod(HttpMethods.Post)
        .WithUrl("https://discord.com/api/webhooks/1029440747482644631/IqUMAzp5APKnzPIL3URskC8KYkvLrgscvdHUYDxPa8VJBdagFqzJTYIpav4C3OiMN3ll");
    }
    else
    {
      httpRequest
        .WithMethod(HttpMethods.Patch)
        .WithUrl("https://discord.com/api/v10/webhooks", interaction.ApplicationId, interaction.Token, "messages/@original");
    }

    httpRequest.WithBody(new
    {
      content = $"<@{interaction.Member.User.Id}> Oopsie... an error occurred: {error}"
    });

    var result = await _httpService.ExecuteAsync<object>(httpRequest.Build());

    _logger.LogInformation($"Message updated: {result.Result}");
  }
}
