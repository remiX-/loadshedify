using Microsoft.Extensions.DependencyInjection;

namespace Proxy.ESP.Api;

public static class EspIoC
{
  public static IServiceCollection WithEspClient(this IServiceCollection collection)
  {
    collection.AddSingleton<IEskomSePushClient, EskomSePushClient>();

    return collection;
  }
}
