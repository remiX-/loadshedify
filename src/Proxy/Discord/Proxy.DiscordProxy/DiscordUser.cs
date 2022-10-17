using System.Text.Json.Serialization;

namespace Proxy.DiscordProxy;

public struct DiscordUser
{
  [JsonPropertyName("id")]
  public string Id { get; set; }

  [JsonPropertyName("username")]
  public string Username { get; set; }

  [JsonPropertyName("avatar")]
  public string Avatar { get; set; }

  [JsonPropertyName("discriminator")]
  public string Discriminator { get; set; }
}
