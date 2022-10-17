namespace Proxy.Core.Commands;

public interface ICommand : IInvocable, IUndoable
{
  void Execute(params object[] args);
}

public interface ICommand<T> : IInvocable, IUndoable
{
  T Execute(params object[] args);
}
