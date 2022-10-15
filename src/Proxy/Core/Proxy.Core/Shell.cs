using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proxy.Core.Model;
using Proxy.Core.Models;
using Proxy.Core.Services;

namespace Proxy.Core;

public static class Shell
{
  private static ServiceCollection _serviceCollection;
  private static ServiceProvider _services;

  public static void ConfigureServices(Action<ServiceCollection> setup = null)
  {
    _serviceCollection = new ServiceCollection();

    _serviceCollection.AddSingleton<IEnvironmentModel, EnvironmentModel>();
    _serviceCollection.AddSingleton<IJsonService, JsonService>();
    _serviceCollection.AddSingleton<IHttpService, HttpService>();
    _serviceCollection.AddSingleton<ITimerService, TimerService>();

    // Default AWS services?
    // _serviceCollection.AddSingleton<IStorageService, S3StorageService>();
    // _serviceCollection.AddSingleton<IDynamoService, DynamoService>();

    _serviceCollection.AddLogging(logging =>
    {
      logging.AddLambdaLogger();
      logging.SetMinimumLevel(LogLevel.Debug);
    });

    setup?.Invoke(_serviceCollection);

    _services = _serviceCollection.BuildServiceProvider();
  }

  public static T Get<T>()
  {
    return _services.GetService<T>() ?? throw new NotImplementedException($"{typeof(T)}");
  }
}
