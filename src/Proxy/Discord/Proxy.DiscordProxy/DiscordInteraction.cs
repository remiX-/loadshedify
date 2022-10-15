using System.Text.Json.Serialization;
using Discord;

namespace Proxy.DiscordProxy;

public struct DiscordInteraction
{
  public bool Dev { get; set; }

  public string Testdata { get; set; }

  [JsonPropertyName("id")]
  public string Id { get; set; }

  [JsonPropertyName("application_id")]
  public string ApplicationId { get; set; }

  [JsonPropertyName("channel_id")]
  public string ChannelId { get; set; }

  [JsonPropertyName("data")]
  public DiscordInteractionData Data { get; set; }

  [JsonPropertyName("member")]
  public DiscordMember Member { get; set; }

  [JsonPropertyName("token")]
  public string Token { get; set; }

  [JsonPropertyName("type")]
  public InteractionType Type { get; set; }
}
