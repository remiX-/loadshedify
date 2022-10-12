using Proxy.Core.DataModels.Web;
using Proxy.Core.Services;
using Proxy.ESP.Api.Entity;

namespace Proxy.ESP.Api;

public class EskomSePushClient : IEskomSePushClient
{
  private readonly IHttpService _httpService;
  private readonly EnvironmentService _envService;

  private const string ApiUrl = "https://developer.sepush.co.za/business/2.0";
  private const string AuthTokenVariable = "ESP_AUTH_TOKEN";

  public EskomSePushClient(IHttpService httpService, EnvironmentService envService)
  {
    _httpService = httpService;
    _envService = envService;
  }

  public async Task<SearchTextResponse> SearchByText(string search)
  {
    var httpRequest = _httpService.NewRequest()
      .WithMethod(HttpMethods.Get)
      .WithUrl(ApiUrl, "areas_search")
      .WithHeader("Token", _envService.Get(AuthTokenVariable))
      .WithQueryParam("text", search)
      .Build();

    var result = await _httpService.ExecuteAsync<SearchTextResponse>(httpRequest);

    return result.Result;
  }

  public async Task<AreaScheduleResponse> GetAreaSchedule(string areaId)
  {
    var httpRequest = _httpService.NewRequest()
      .WithMethod(HttpMethods.Get)
      .WithUrl(ApiUrl, "area")
      .WithHeader("Token", _envService.Get(AuthTokenVariable))
      .WithQueryParam("id", areaId)
      .Build();

    var result = await _httpService.ExecuteAsync<AreaScheduleResponse>(httpRequest);

    return result.Result;
  }
}
