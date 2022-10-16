using Discord;
using Microsoft.Extensions.Logging;
using Proxy.Core.DataModels.Web;
using Proxy.Core.Model;
using Proxy.Core.Services;

namespace Proxy.DiscordProxy;

public class DiscordHandler
{
  private readonly IHttpService _httpService;
  private readonly IEnvironmentModel _envModel;
  private readonly ILogger<DiscordHandler> _logger;

  public DiscordHandler(IHttpService httpService,
    IEnvironmentModel envModel,
    ILogger<DiscordHandler> logger)
  {
    _httpService = httpService;
    _envModel = envModel;
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
