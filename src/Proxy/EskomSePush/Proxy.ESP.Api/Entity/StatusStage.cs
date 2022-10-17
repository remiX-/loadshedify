using System.Text.Json.Serialization;

namespace Proxy.ESP.Api.Entity;

public struct StatusStage
{
  public string Stage { get; init; }

  [JsonPropertyName("stage_start_timestamp")]
  public DateTimeOffset Timestamp { get; init; }
}
