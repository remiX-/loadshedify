using Discord;
using Discord.Rest;
using Microsoft.Extensions.Logging;
using Proxy.Core.DataModels.Web;
using Proxy.Core.Services;

namespace Proxy.DiscordProxy;

public class DiscordHandler
{
  private readonly IHttpService _httpService;
  private readonly ILogger<DiscordHandler> _logger;

  public DiscordHandler(IHttpService httpService, ILogger<DiscordHandler> logger)
  {
    _httpService = httpService;
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
        footer.IconUrl = $"https://{Environment.GetEnvironmentVariable("S3_ASSET_BUCKET")}.s3.eu-west-1.amazonaws.com/assets/images/electricity.png";
      });

    var httpRequest = _httpService.NewRequest()
      .WithMethod(HttpMethods.Patch)
      .WithUrl("https://discord.com/api/v10/webhooks", interaction.ApplicationId, interaction.Token, "messages/@original")
      .WithBody(new
      {
        content = $"<@{interaction.Member.User.Id}>",
        embeds = new List<DiscordEmbed>()
        {
          new DiscordEmbed(embed.Build())
        }
      })
      .Build();

    var result = await _httpService.ExecuteAsync<object>(httpRequest);

    _logger.LogInformation("Message updated.");
  }
}
