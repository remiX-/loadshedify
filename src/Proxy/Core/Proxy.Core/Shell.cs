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

    MapModels(_serviceCollection);
    MapServices(_serviceCollection);
    MapTestData(_serviceCollection);
    MapAwsServices(_serviceCollection);

    _serviceCollection.AddLogging(logging =>
    {
      logging.AddLambdaLogger();
      logging.SetMinimumLevel(LogLevel.Debug);
    });

    setup?.Invoke(_serviceCollection);

    _services = _serviceCollection.BuildServiceProvider();
  }

  private static void MapModels(IServiceCollection sc)
  {
    sc.AddSingleton<IEnvironmentModel, EnvironmentModel>();
    sc.AddSingleton<IVariablesModel>(sp =>
    {
      var envModel = sp.GetService<IEnvironmentModel>()!;

      return new VariablesModel
      {
        AspNetEnvironment = envModel.Get("ASPNETCORE_ENVIRONMENT", false),
        DebugEnabled = envModel.GetBool("DEBUG_ENABLED", false),
        EspAuthToken = envModel.Get("ESP_AUTH_TOKEN"),
        S3AssetBucket = envModel.Get("S3_ASSET_BUCKET"),
      };
    });
  }

  private static void MapServices(IServiceCollection sc)
  {
    sc.AddSingleton<IJsonService, JsonService>();
    sc.AddSingleton<IHttpService, HttpService>();
    sc.AddSingleton<ITimerService, TimerService>();
  }

  private static void MapTestData(IServiceCollection sc)
  {
    sc.AddSingleton<TestDataProvider>();
  }

  private static void MapAwsServices(IServiceCollection sc)
  {
    // Default AWS services?
    // _serviceCollection.AddSingleton<IStorageService, S3StorageService>();
    // _serviceCollection.AddSingleton<IDynamoService, DynamoService>();
  }

  public static T Get<T>()
  {
    return _services.GetService<T>() ?? throw new NotImplementedException($"{typeof(T)}");
  }
}
