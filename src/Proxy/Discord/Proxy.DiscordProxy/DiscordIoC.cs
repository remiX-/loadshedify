using Microsoft.Extensions.DependencyInjection;

namespace Proxy.DiscordProxy;

public static class DiscordIoC
{
  public static IServiceCollection WithDiscordHandler(this IServiceCollection collection)
  {
    collection.AddSingleton<DiscordHandler>();

    return collection;
  }
}
