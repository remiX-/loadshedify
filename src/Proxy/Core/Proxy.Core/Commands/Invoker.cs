using Microsoft.Extensions.Logging;
using Proxy.Core.Services;

namespace Proxy.Core.Commands;

public class Invoker : IInvoker
{
  private static TimerService _timerService;
  private static ILogger<Invoker> _logger;

  public Invoker(TimerService timerService, ILogger<Invoker> logger)
  {
    _timerService = timerService;
    _logger = logger;
  }

  public void Execute<TCommand>(params object[] args) where TCommand : ICommand
  {
    var command = Shell.Get<TCommand>();
    var commandId = command.GetType().Name;

    _timerService.Start(commandId);

    command.Execute(args);

    var duration = _timerService.End(commandId);
    _logger.LogDebug($"{commandId}.duration: {duration.TotalSeconds}s / {duration.TotalMilliseconds}ms");
  }

  public TResult Execute<TCommand, TResult>(params object[] args) where TCommand : ICommand<TResult>
  {
    var command = Shell.Get<TCommand>();
    var commandId = command.GetType().Name;

    _timerService.Start(commandId);

    var result = command.Execute(args);

    var duration = _timerService.End(commandId);
    _logger.LogDebug($"{commandId}.duration: {duration.TotalSeconds}s / {duration.TotalMilliseconds}ms");

    return result;
  }

  public async Task ExecuteAsync<TCommand>(CancellationToken cancellationToken, params object[] args) where TCommand : IAsyncCommand
  {
    var command = Shell.Get<TCommand>();
    var commandId = command.GetType().Name;

    _timerService.Start(commandId);
    command.Invoker = this;
    await command.Execute(cancellationToken, args);

    var duration = _timerService.End(commandId);
    _logger.LogDebug($"{commandId}.duration: {duration.TotalSeconds}s / {duration.TotalMilliseconds}ms");
  }

  public async Task<TResult> ExecuteAsync<TCommand, TResult>(CancellationToken cancellationToken, params object[] args) where TCommand : IAsyncCommand<TResult>
  {
    var command = Shell.Get<TCommand>();
    var commandId = command.GetType().Name;

    _timerService.Start(commandId);

    var result = await command.Execute(cancellationToken, args);

    var duration = _timerService.End(commandId);
    _logger.LogDebug($"{commandId}.duration: {duration.TotalSeconds}s / {duration.TotalMilliseconds}ms");

    return result;
  }
}
