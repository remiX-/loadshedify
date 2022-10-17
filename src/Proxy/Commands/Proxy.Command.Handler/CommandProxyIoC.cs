using Microsoft.Extensions.DependencyInjection;

namespace Proxy.Command.Handler;

public static class CommandProxyIoC
{
  public static IServiceCollection WithCommandProxy(this IServiceCollection collection)
  {
    collection.AddSingleton<CommandHandler>();

    return collection;
  }
}
