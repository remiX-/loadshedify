using Proxy.ESP.Api.Entity;

namespace Proxy.ESP.Api.Response;

public struct AreaScheduleResponse
{
  public IList<AreaScheduleEvent> Events { get; init; }

  public AreaScheduleInfo Info { get; init; }

  public AreaSchedule Schedule { get; init; }
}
