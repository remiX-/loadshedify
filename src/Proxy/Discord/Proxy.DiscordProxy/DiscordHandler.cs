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

  public async Task Handle(DiscordInteraction interaction, IReadOnlyList<EmbedBuilder> embeds)
  {
#if DEBUG
    if (interaction.Dev)
    {
      var devChannelId = _envModel.Get("BOT_DEV_CHANNEL_ID", false);
      if (devChannelId is not null)
      {
        await SendMessage(devChannelId, $"DEV MODE - <@{interaction.Member.User.Id}>", embeds);
        return;
      }
    }
#endif

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
      .WithMethod(HttpMethods.Patch)
      .WithUrl("https://discord.com/api/v10/webhooks", interaction.ApplicationId, interaction.Token, "messages/@original")
      .WithBody(new
      {
        content = $"<@{interaction.Member.User.Id}>",
        embeds = embeds.Select(embed => new DiscordEmbed(embed.Build()))
      });

    await _httpService.ExecuteAsync<object>(httpRequest.Build());

    _logger.LogInformation("Message updated");
  }

  public async Task SendMessage(string channelId, string content, IReadOnlyList<EmbedBuilder> embeds)
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
        content = content,
        embeds = embeds.Select(embed => new DiscordEmbed(embed.Build()))
      });

    _logger.LogDebug(_jsonService.Serialize(httpRequest.Build()));

    await _httpService.ExecuteAsync<object>(httpRequest.Build());

    _logger.LogInformation("Message sent");
  }

  public async Task SendMessage(string channelId, string content, EmbedBuilder embed)
  {
    await SendMessage(channelId, content, new List<EmbedBuilder> { embed });
  }

  public async Task HandleError(DiscordInteraction interaction, string error)
  {
#if DEBUG
    if (interaction.Dev)
    {
      var devChannelId = _envModel.Get("BOT_DEV_CHANNEL_ID", false);
      if (devChannelId is not null)
      {
        await SendMessage(devChannelId, $"DEV MODE - <@{interaction.Member.User.Id}> Oopsie... an error occurred: {error}", new List<EmbedBuilder>());
        return;
      }
    }
#endif

    var httpRequest = _httpService.NewRequest()
        .WithMethod(HttpMethods.Patch)
        .WithUrl("https://discord.com/api/v10/webhooks", interaction.ApplicationId, interaction.Token, "messages/@original")
        .WithBody(new
        {
          content = $"<@{interaction.Member.User.Id}> Oopsie... an error occurred: {error}"
        });

    var result = await _httpService.ExecuteAsync<object>(httpRequest.Build());

    _logger.LogError($"An error occurred: {result.Result}");
  }
}
