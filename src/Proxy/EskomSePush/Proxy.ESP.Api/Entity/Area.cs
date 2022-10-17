namespace Proxy.ESP.Api.Entity;

public struct Area
{
  public string Id { get; init; }
  public string Name { get; init; }
  public string Region { get; init; }

  public void Deconstruct(out string id, out string name, out string region)
  {
    id = Id;
    name = Name;
    region = Region;
  }
}
