namespace Proxy.Core.DataModels.Web;

public interface IWebExecuteResponse
{
  public bool Success { get; }
  public Exception Exception { get; }

  public string Body { get; }
}
