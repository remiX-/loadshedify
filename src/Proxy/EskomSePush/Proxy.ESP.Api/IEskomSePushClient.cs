using Proxy.ESP.Api.Entity;
using Proxy.ESP.Api.Response;

namespace Proxy.ESP.Api;

public interface IEskomSePushClient
{
  Task<StatusResponse> Status();

  Task<SearchTextResponse> SearchByText(string search);

  Task<AreaScheduleResponse> GetAreaSchedule(string areaId, string? simTest = null);
}
