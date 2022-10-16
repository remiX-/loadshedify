using Microsoft.Extensions.Logging;
using Proxy.Core.DataModels.Web;
using Proxy.Core.Model;
using Proxy.Core.Services;
using Proxy.ESP.Api.Response;

namespace Proxy.ESP.Api;

public class EskomSePushClient : IEskomSePushClient
{
  private readonly IHttpService _httpService;
  private readonly IEnvironmentModel _envModel;
  private readonly IVariablesModel _varModel;
  private readonly ILogger<EskomSePushClient> _logger;

  private const string ApiUrl = "https://developer.sepush.co.za/business/2.0";
  private const string EspAuthTokenVar = "ESP_AUTH_TOKEN";

  public EskomSePushClient(IHttpService httpService,
    IEnvironmentModel envModel,
    IVariablesModel varModel,
    ILogger<EskomSePushClient> logger)
  {
    _httpService = httpService;
    _envModel = envModel;
    _varModel = varModel;
    _logger = logger;
  }

  public async Task<StatusResponse> Status()
  {
    var httpRequest = GetDefaultRequest()
      .AppendUrl("status");

    var result = await _httpService.ExecuteAsync<StatusResponse>(httpRequest.Build());

    if (_varModel.DebugEnabled)
    {
      _logger.LogDebug(result.Response.Body);
    }

    return result.Result;
  }

  public async Task<SearchTextResponse> SearchByText(string search)
  {
    var httpRequest = GetDefaultRequest()
      .AppendUrl("areas_search")
      .WithQueryParam("text", search);

    var result = await _httpService.ExecuteAsync<SearchTextResponse>(httpRequest.Build());

    if (_varModel.DebugEnabled)
    {
      _logger.LogDebug(result.Response.Body);
    }

    return result.Result;
  }

  public async Task<AreaScheduleResponse> GetAreaSchedule(string areaId, string? simTest = null)
  {
    var httpRequest = GetDefaultRequest()
      .AppendUrl("area")
      .WithQueryParam("id", areaId);

    if (simTest is not null)
    {
      httpRequest.WithQueryParam("test", simTest);
    }

    var result = await _httpService.ExecuteAsync<AreaScheduleResponse>(httpRequest.Build());

    if (_varModel.DebugEnabled)
    {
      _logger.LogDebug(result.Response.Body);
    }

    return result.Result;
  }

  private IHttpRequestBuilder GetDefaultRequest()
  {
    return _httpService.NewRequest()
      .WithMethod(HttpMethods.Get)
      .WithUrl(ApiUrl)
      .WithHeader("Token", _envModel.Get(EspAuthTokenVar));
  }
}
