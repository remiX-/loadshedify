using System.Text.Json.Serialization;
using Proxy.ESP.Api.Entity;

namespace Proxy.ESP.Api.Response;

public struct AreaScheduleResponse
{
  [JsonIgnore]
  public string Id { get; set; }

  public IList<AreaScheduleEvent> Events { get; init; }

  public AreaScheduleInfo Info { get; init; }

  public AreaSchedule Schedule { get; init; }
}
