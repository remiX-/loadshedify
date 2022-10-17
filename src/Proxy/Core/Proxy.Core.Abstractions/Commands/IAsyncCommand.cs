namespace Proxy.Core.Commands;

public interface IAsyncCommand : IInvocable, IUndoable
{
  Task Execute(CancellationToken cancellationToken = default, params object[] args);
}

public interface IAsyncCommand<T> : IInvocable, IUndoable
{
  Task<T> Execute(CancellationToken cancellationToken = default, params object[] args);
}
