namespace Proxy.Core.Commands;

public interface IInvocable
{
  IInvoker Invoker { get; set; }
}
