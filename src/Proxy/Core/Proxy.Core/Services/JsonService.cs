using System.Text.Encodings.Web;
using System.Text.Json;

namespace Proxy.Core.Services;

public class JsonService : IJsonService
{
  public JsonService()
  { }

  public T Deserialize<T>(string json)
  {
    if (json is null)
    {
      throw new Exception("json is null");
    }

    return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });
  }

  public bool TryDeserialize<T>(string json, out T deserialized)
  {
    deserialized = default;

    try
    {
      deserialized = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      });

      return true;
    }
    catch
    {
      return false;
    }
  }

  public string Serialize(object obj, bool prettify = false)
  {
    try
    {
      return JsonSerializer.Serialize(obj, new JsonSerializerOptions
      {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = prettify
      });
    }
    catch
    {
      return null;
    }
  }

  public dynamic DebugObject(object obj)
  {
    var debugSerialize = Serialize(obj);
    return Deserialize<dynamic>(debugSerialize);
  }
}
