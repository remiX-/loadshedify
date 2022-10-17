namespace Proxy.Core.DataModels.Web;

public interface IRequestResult<T>
{
  public bool Success { get; }
  public string Error { get; }
  public IWebExecuteResponse Response { get; }

  public T Result { get; }
}
