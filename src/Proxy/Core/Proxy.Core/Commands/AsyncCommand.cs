namespace Proxy.Core.Commands;

public abstract class AsyncCommand : IAsyncCommand
{
  public IInvoker Invoker { get; set; }

  public abstract Task Execute(CancellationToken cancellationToken = default, params object[] args);

  public abstract void Undo();
}

public abstract class AsyncCommand<T> : IAsyncCommand<T>
{
  public IInvoker Invoker { get; set; }

  public abstract Task<T> Execute(CancellationToken cancellationToken = default, params object[] args);

  public abstract void Undo();
}
