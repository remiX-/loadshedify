using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Discord;

namespace Proxy.DiscordProxy;

public class DiscordEmbed
{
  [JsonIgnore] private readonly Embed _embed;

  public EmbedType Type => _embed.Type;

  public string Description => _embed.Description;

  public string Url => _embed.Url;

  public string Title => _embed.Title;

  public DateTimeOffset? Timestamp => _embed.Timestamp;

  public uint? Color => _embed.Color;

  public EmbedImage? Image => _embed.Image;

  public EmbedVideo? Video => _embed.Video;

  public EmbedAuthor? Author => _embed.Author;

  public EmbedFooter? Footer => _embed.Footer;

  public EmbedProvider? Provider => _embed.Provider;

  public EmbedThumbnail? Thumbnail => _embed.Thumbnail;

  public ImmutableArray<EmbedField> Fields => _embed.Fields;

  public DiscordEmbed(Embed embed)
  {
    _embed = embed;
  }
}
