using System.Net;

namespace Proxy.Core.DataModels.Web;

public class WebExecuteResponse : IWebExecuteResponse
{
  public HttpStatusCode StatusCode { get; set; }

  public bool Success { get; set; }

  public Exception Exception { get; set; }

  public string Body { get; set; }
}
