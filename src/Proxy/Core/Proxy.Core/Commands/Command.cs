namespace Proxy.Core.Commands;

public abstract class Command : ICommand
{
  public IInvoker Invoker { get; set; }

  public abstract void Execute(params object[] args);

  public abstract void Undo();
}

public abstract class Command<T> : ICommand<T>
{
  public IInvoker Invoker { get; set; }

  public abstract T Execute(params object[] args);

  public abstract void Undo();
}
