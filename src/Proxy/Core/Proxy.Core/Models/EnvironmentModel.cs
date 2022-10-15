using Proxy.Core.Model;

namespace Proxy.Core.Models;

public class EnvironmentModel : IEnvironmentModel
{
  public string Get(string key)
  {
    return Environment.GetEnvironmentVariable(key) ?? throw new KeyNotFoundException($"Failed to retrieve environment variable: {key}");
  }
}
