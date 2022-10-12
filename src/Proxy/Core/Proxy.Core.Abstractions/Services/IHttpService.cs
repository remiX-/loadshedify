using Proxy.Core.DataModels.Web;

namespace Proxy.Core.Services;

public interface IHttpService
{
  Task<IRequestResult<T>> ExecuteAsync<T>(IHttpRequest request);

  Task<IRequestResult<T>> ExecuteAsync<T>(string verb, string url, Dictionary<string, string> headers = null);

  Task<IRequestResult<T>> ExecuteAsync<T>(string verb, string url, object body);

  IHttpRequestBuilder NewRequest();
}
