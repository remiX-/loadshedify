using Proxy.ESP.Api.Entity;

namespace Proxy.ESP.Api.Response;

public struct SearchTextResponse
{
  public IList<Area> Areas { get; init; }
}
