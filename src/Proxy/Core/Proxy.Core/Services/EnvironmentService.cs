namespace Proxy.Core.Services;

public class EnvironmentService
{
  // public string AspNetEnvironment { get; set; }
  // public string Environment { get; set; }
  // public bool IsDevelopment => Environment == Environments.Development;
  // public bool IsProduction => Environment == Environments.Production;

  public EnvironmentService()
  {
  }

  public string Get(string key)
  {
    return System.Environment.GetEnvironmentVariable(key) ?? throw new KeyNotFoundException(key);
  }
}
