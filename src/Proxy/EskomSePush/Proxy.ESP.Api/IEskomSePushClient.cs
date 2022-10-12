using Proxy.ESP.Api.Entity;

namespace Proxy.ESP.Api;

public interface IEskomSePushClient
{
  Task<SearchTextResponse> SearchByText(string search);
}
