namespace Proxy.Core.DataModels.Web;

public interface IHttpRequestBuilder
{
  IHttpRequestBuilder WithMethod(string method);
  IHttpRequestBuilder WithUrl(params string[] url);
  IHttpRequestBuilder AppendUrl(params string[] url);
  IHttpRequestBuilder WithHeader(string key, string value);
  IHttpRequestBuilder WithQueryParam(string queryParam, string value);
  IHttpRequestBuilder WithBody(object body);

  IHttpRequest Build();
}
