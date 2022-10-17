using Proxy.Core.Model;

namespace Proxy.Core.Models;

public class EnvironmentModel : IEnvironmentModel
{
  public string Get(string key, bool throwOnNotFound = true)
  {
    var envVar = Environment.GetEnvironmentVariable(key);

    if (envVar is not null) return envVar;

    if (throwOnNotFound) throw new KeyNotFoundException($"Failed to retrieve environment variable: {key}");

    Console.WriteLine($"Could not find variable: {key}");
    return null;
  }

  public bool GetBool(string key, bool defaultValue)
  {
    var envVar = Environment.GetEnvironmentVariable(key);
    if (envVar is null) return defaultValue;

    return bool.Parse(envVar);
  }

  public int GetInt(string key, int defaultValue)
  {
    var envVar = Environment.GetEnvironmentVariable(key);
    if (envVar is null) return defaultValue;

    return int.Parse(envVar);
  }
}
