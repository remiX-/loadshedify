using System.Net.Http.Headers;
using Proxy.Core.DataModels.Web;

namespace Proxy.Core.Services;

public class HttpService : IHttpService
{
  private readonly JsonService _jsonService;
  private readonly HttpClient _httpClient;

  public HttpService(JsonService jsonService)
  {
    _jsonService = jsonService;

    _httpClient = new HttpClient();
  }

  public async Task<IRequestResult<T>> ExecuteAsync<T>(IHttpRequest request)
  {
    var response = await Run(request);

    if (!response.Success)
    {
      return new RequestResult<T> { Success = false, Response = response, Error = response.Exception.Message };
    }

    var deserialized = _jsonService.Deserialize<T>(response.Body);
    return new RequestResult<T> { Success = true, Response = response, Result = deserialized };
  }

  public async Task<IRequestResult<T>> ExecuteAsync<T>(string verb, string url, Dictionary<string, string> headers = null)
  {
    var response = await Run(verb, url, headers);

    if (!response.Success)
    {
      return new RequestResult<T> { Success = false, Response = response, Error = response.Exception.Message };
    }

    var deserialized = _jsonService.Deserialize<T>(response.Body);
    return new RequestResult<T> { Success = true, Response = response, Result = deserialized };
  }

  public async Task<IRequestResult<T>> ExecuteAsync<T>(string verb, string url, object body)
  {
    var response = await Run(verb, url, null, body);

    if (!response.Success)
    {
      return new RequestResult<T> { Success = false, Response = response, Error = response.Exception.Message };
    }

    var deserialized = _jsonService.Deserialize<T>(response.Body);
    return new RequestResult<T> { Success = true, Response = response, Result = deserialized };
  }

  public IHttpRequestBuilder NewRequest() => new HttpRequestBuilder();

  internal async Task<WebExecuteResponse> Run(string verb, string url, Dictionary<string, string> headers = null, object body = null)
  {
    try
    {
      _httpClient.DefaultRequestHeaders.Clear();
      // _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");

      if (headers?.Count > 0)
      {
        foreach (var kvp in headers)
        {
          _httpClient.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
        }
      }

      var response = verb switch
      {
        HttpMethods.Get => await _httpClient.GetAsync(url),
        HttpMethods.Post => await _httpClient.PostAsync(url, GetBodyJson(body)),
        HttpMethods.Patch => await _httpClient.PatchAsync(url, GetBodyJson(body)),
        _ => throw new ArgumentOutOfRangeException(nameof(verb), verb, null)
      };

      return new WebExecuteResponse
      {
        Success = true,
        Body = await response.Content.ReadAsStringAsync()
      };
    }
    catch (Exception error)
    {
      return new WebExecuteResponse
      {
        Success = false,
        Exception = error
      };
    }
  }

  internal async Task<WebExecuteResponse> Run(IHttpRequest request)
  {
    try
    {
      _httpClient.DefaultRequestHeaders.Clear();


      if (request.Headers.Count > 0)
      {
        foreach (var kvp in request.Headers)
        {
          _httpClient.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
        }
      }

      var response = request.Method switch
      {
        HttpMethods.Get => await _httpClient.GetAsync(request.Url),
        HttpMethods.Post => await _httpClient.PostAsync(request.Url, GetBodyJson(request.Body)),
        HttpMethods.Put => await _httpClient.PutAsync(request.Url, GetBodyJson(request.Body)),
        HttpMethods.Patch => await _httpClient.PatchAsync(request.Url, GetBodyJson(request.Body)),
        _ => throw new ArgumentOutOfRangeException(nameof(request.Method), request.Method, null)
      };

      return new WebExecuteResponse
      {
        Success = true,
        Body = await response.Content.ReadAsStringAsync()
      };
    }
    catch (Exception error)
    {
      return new WebExecuteResponse
      {
        Success = false,
        Exception = error
      };
    }
  }

  private ByteArrayContent GetBodyJson(object body)
  {
    if (body is null) return null;

    var jsonStringify = _jsonService.Serialize(body);
    var buffer = System.Text.Encoding.UTF8.GetBytes(jsonStringify);
    var byteContent = new ByteArrayContent(buffer);
    byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

    return byteContent;
  }
}
