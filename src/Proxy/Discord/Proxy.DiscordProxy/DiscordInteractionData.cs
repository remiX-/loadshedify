namespace Proxy.DiscordProxy;

public struct DiscordInteractionData
{
  public string Id { get; set; }

  public string Name { get; set; }

  public IList<DiscordDataOption> Options { get; set; }
}
