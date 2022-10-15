using Discord;

namespace Proxy.DiscordProxy.Extensions;

public static class EmbedBuilderExtensions
{
  public static EmbedBuilder AddEmptyField(this EmbedBuilder eb)
  {
    return eb.AddField("\u200B", "\u200B");
  }

  public static EmbedBuilder AddInlineEmptyField(this EmbedBuilder eb)
  {
    return eb.AddField("\u200B", "\u200B", true);
  }
}
