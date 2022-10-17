namespace Proxy.BotHandler;

internal struct DiscordProxyRequest
{
  public int Type { get; init; }

  public string Id { get; init; }

  public DiscordCommandData Data { get; init; }

  public string Token { get; init; }
}
