using Proxy.ESP.Api.Entity;

namespace Proxy.ESP.Api.Response;

public struct StatusResponse
{
  public IDictionary<string, Status> Status { get; init; }
}
