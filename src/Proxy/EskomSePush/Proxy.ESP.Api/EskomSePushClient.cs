using Proxy.Core.DataModels.Web;
using Proxy.Core.Model;
using Proxy.Core.Services;
using Proxy.ESP.Api.Response;

namespace Proxy.ESP.Api;

public class EskomSePushClient : IEskomSePushClient
{
  private readonly IHttpService _httpService;
  private readonly IEnvironmentModel _envService;

  private const string ApiUrl = "https://developer.sepush.co.za/business/2.0";
  private const string AuthTokenVariable = "ESP_AUTH_TOKEN";

  public EskomSePushClient(IHttpService httpService, IEnvironmentModel envService)
  {
    _httpService = httpService;
    _envService = envService;
  }

  public async Task<StatusResponse> Status()
  {
    var httpRequest = GetDefaultRequest()
      .AppendUrl("status")
      .Build();

    var result = await _httpService.ExecuteAsync<StatusResponse>(httpRequest);

    return result.Result;
  }

  public async Task<SearchTextResponse> SearchByText(string search)
  {
    var httpRequest = GetDefaultRequest()
      .AppendUrl("areas_search")
      .WithQueryParam("text", search)
      .Build();

    var result = await _httpService.ExecuteAsync<SearchTextResponse>(httpRequest);

    return result.Result;
  }

  public async Task<AreaScheduleResponse> GetAreaSchedule(string areaId)
  {
    var httpRequest = GetDefaultRequest()
      .AppendUrl("area")
      .WithQueryParam("id", areaId)
      .Build();

    var result = await _httpService.ExecuteAsync<AreaScheduleResponse>(httpRequest);

    return result.Result;
  }

  private IHttpRequestBuilder GetDefaultRequest()
  {
    return _httpService.NewRequest()
      .WithMethod(HttpMethods.Get)
      .WithUrl(ApiUrl)
      .WithHeader("Token", _envService.Get(AuthTokenVariable));
  }
}
