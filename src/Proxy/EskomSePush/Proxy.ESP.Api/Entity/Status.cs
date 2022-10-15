using System.Text.Json.Serialization;

namespace Proxy.ESP.Api.Entity;

public struct Status
{
  public string Name { get; init; }

  public string Stage { get; init; }

  [JsonPropertyName("stage_updated")]
  public DateTime Updated { get; init; }

  [JsonPropertyName("next_stages")]
  public IList<StatusStage> NextStages { get; init; }
}
