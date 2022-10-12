namespace Proxy.Core.DataModels.Web;

public class RequestResult<T> : IRequestResult<T>
{
  public bool Success { get; set; }
  public string Error { get; set; }
  public IWebExecuteResponse Response { get; set; }

  public T Result { get; set; }
}
