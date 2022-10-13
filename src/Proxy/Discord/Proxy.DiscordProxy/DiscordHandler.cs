﻿using Discord;
using Microsoft.Extensions.Logging;
using Proxy.Core.DataModels.Web;
using Proxy.Core.Services;

namespace Proxy.DiscordProxy;

public class DiscordHandler
{
  private readonly IHttpService _httpService;
  private readonly EnvironmentService _envService;
  private readonly ILogger<DiscordHandler> _logger;

  public DiscordHandler(IHttpService httpService,
    EnvironmentService envService,
    ILogger<DiscordHandler> logger)
  {
    _httpService = httpService;
    _envService = envService;
    _logger = logger;
  }

  public async Task Handle(DiscordInteraction interaction, EmbedBuilder embed)
  {
    // Add defaults to embed
    embed
      .WithCurrentTimestamp()
      .WithFooter(footer =>
      {
        footer.Text = "Developed by remiX";
        // TODO convert to S3 Service
        footer.IconUrl = $"https://{_envService.Get("S3_ASSET_BUCKET")}.s3.eu-west-1.amazonaws.com/assets/images/electricity.png";
      });

    var httpRequest = _httpService.NewRequest();

    if (interaction.Dev)
    {
      httpRequest
        .WithMethod(HttpMethods.Post)
        .WithUrl(
          "https://discord.com/api/webhooks/1029440747482644631/IqUMAzp5APKnzPIL3URskC8KYkvLrgscvdHUYDxPa8VJBdagFqzJTYIpav4C3OiMN3ll");
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
      embeds = new List<DiscordEmbed>
      {
        new(embed.Build())
      }
    });

    var result = await _httpService.ExecuteAsync<object>(httpRequest.Build());

    _logger.LogInformation($"Message updated: {result.Result}");
  }
}
