using Proxy.Core.Utils;
using static System.Net.WebRequestMethods;

namespace Proxy.Core.DataModels.Web;

public class HttpRequestBuilder : IHttpRequestBuilder
{
  public string Method { get; private set; }
  public string Url { get; private set; }
  public Dictionary<string, string> Headers { get; }
  public object Body { get; private set; }

  public HttpRequestBuilder(string method, string url)
  {
    Method = method;
    Url = url;
    Headers = new Dictionary<string, string>();
  }

  public HttpRequestBuilder(string verb) : this(verb, null)
  { }

  public HttpRequestBuilder() : this(Http.Get, null)
  { }

  public IHttpRequestBuilder WithMethod(string method)
  {
    Method = method;

    return this;
  }

  public IHttpRequestBuilder WithUrl(params string[] url)
  {
    Url = UrlHelpers.SafeUrl(url);

    return this;
  }

  public IHttpRequestBuilder AppendUrl(params string[] url)
  {
    Url = UrlHelpers.SafeUrl(Url, url);

    return this;
  }

  public IHttpRequestBuilder WithHeader(string key, string value)
  {
    Headers.Add(key, value);

    return this;
  }

  public IHttpRequestBuilder WithQueryParam(string queryParam, string value)
  {
    if (Url is null) throw new NullReferenceException("Base Url is null");

    if (Url.Contains('?'))
    {
      Url += $"&{queryParam}={value}";
    }
    else
    {
      Url += $"?{queryParam}={value}";
    }

    return this;
  }

  public IHttpRequestBuilder WithBody(object body)
  {
    Body = body;

    return this;
  }

  public IHttpRequest Build()
  {
    return new HttpRequest
    {
      Method = Method,
      Url = Url,
      Headers = Headers,
      Body = Body
    };
  }
}
