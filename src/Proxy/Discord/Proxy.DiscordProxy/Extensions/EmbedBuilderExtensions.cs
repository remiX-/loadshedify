using Discord;

namespace Proxy.DiscordProxy.Extensions;

public static class EmbedBuilderExtensions
{
  public static string EmptyString = "​​​​\u200B";

  public static EmbedBuilder AddEmptyField(this EmbedBuilder eb)
  {
    return eb.AddField(EmptyString, EmptyString);
  }

  public static EmbedBuilder AddInlineEmptyField(this EmbedBuilder eb)
  {
    return eb.AddField(EmptyString, EmptyString, true);
  }

  public static EmbedBuilder AddEmptyFieldWithName(this EmbedBuilder eb, string name)
  {
    return eb.AddField(name, EmptyString);
  }
}
