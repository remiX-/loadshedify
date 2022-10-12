namespace Proxy.Core.DataModels.Web;

public class HttpRequest : IHttpRequest
{
  public string Method { get; set; }
  public string Url { get; set; }
  public Dictionary<string, string> Headers { get; set; }
  public object Body { get; set; }
}
