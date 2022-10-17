namespace Proxy.Core.Commands;

public interface IInvoker
{
  void Execute<TCommand>(params object[] args) where TCommand : ICommand;
  TResult Execute<TCommand, TResult>(params object[] args) where TCommand : ICommand<TResult>;

  Task ExecuteAsync<TCommand>(CancellationToken cancellationToken, params object[] args) where TCommand : IAsyncCommand;
  Task<TResult> ExecuteAsync<TCommand, TResult>(CancellationToken cancellationToken, params object[] args) where TCommand : IAsyncCommand<TResult>;
}
