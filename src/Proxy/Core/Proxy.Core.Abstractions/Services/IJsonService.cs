namespace Proxy.Core.Services;

public interface IJsonService
{
  T Deserialize<T>(string json);

  bool TryDeserialize<T>(string json, out T deserialized);

  string Serialize(object obj, bool prettify = false);

  dynamic DebugObject(object obj);
}
