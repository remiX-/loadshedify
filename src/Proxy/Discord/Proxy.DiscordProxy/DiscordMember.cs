using System.Text.Json.Serialization;

namespace Proxy.DiscordProxy;

public struct DiscordMember
{
  [JsonPropertyName("user")]
  public DiscordUser User { get; set; }
}
