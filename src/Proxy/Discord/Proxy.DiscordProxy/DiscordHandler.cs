using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Proxy.Core.DataModels.Web;
using Proxy.Core.Model;
using Proxy.Core.Services;

namespace Proxy.DiscordProxy;

public class DiscordHandler
{
  private readonly IHttpService _httpService;
  private readonly IEnvironmentModel _envModel;
  private readonly IJsonService _jsonService;
  private readonly ILogger<DiscordHandler> _logger;

  public DiscordHandler(IHttpService httpService,
    IEnvironmentModel envModel,
    IJsonService jsonService,
    ILogger<DiscordHandler> logger)
  {
    _httpService = httpService;
    _envModel = envModel;
    _jsonService = jsonService;
    _logger = logger;
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
          // TODO convert to S3 Service?
          footer.IconUrl = $"https://{_envModel.Get("S3_ASSET_BUCKET")}.s3.eu-west-1.amazonaws.com/assets/images/electricity.png";
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

    await _httpService.ExecuteAsync<object>(httpRequest.Build());

    _logger.LogInformation("Message updated");
  }

  public async Task SendMessage(IList<string> userIds, string channelId, IReadOnlyList<EmbedBuilder> embeds)
  {
    // Add defaults to embed
    foreach (var embed in embeds)
    {
      embed
        .WithCurrentTimestamp()
        .WithFooter(footer =>
        {
          footer.Text = "Developed by remiX";
          // TODO convert to S3 Service?
          footer.IconUrl = $"https://{_envModel.Get("S3_ASSET_BUCKET")}.s3.eu-west-1.amazonaws.com/assets/images/electricity.png";
        });
    }

    var httpRequest = _httpService.NewRequest()
      .WithMethod(HttpMethods.Post)
      .WithHeader("Authorization", $"Bot {_envModel.Get("DISCORD_BOT_TOKEN")}")
      .WithUrl("https://discord.com/api/v10/channels/", channelId, "messages")
      .WithBody(new
      {
        // content = $"<@{userId}>",
        content = $"{userIds.Select(userId => $"<@{userId}>").Aggregate((c, n) => $"{c} {n}")}",
        embeds = embeds.Select(embed => new DiscordEmbed(embed.Build()))
      });

    _logger.LogDebug(_jsonService.Serialize(httpRequest.Build()));

    await _httpService.ExecuteAsync<object>(httpRequest.Build());

    _logger.LogInformation("Message sent");
  }

  public async Task SendMessage(IList<string> userIds, string channelId, EmbedBuilder embed)
  {
    await SendMessage(userIds, channelId, new List<EmbedBuilder> { embed });
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

    _logger.LogError($"An error occurred: {result.Result}");
  }
}
