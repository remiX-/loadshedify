namespace Proxy.Core.DataModels.Web;

public interface IHttpRequest
{
  public string Method { get; }
  public string Url { get; }
  public Dictionary<string, string> Headers { get; }
  public object Body { get; }
}
