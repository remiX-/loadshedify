namespace Proxy.Core.Model;

public interface IEnvironmentModel
{
  string Get(string key, bool throwOnNotFound = true);

  bool GetBool(string key, bool defaultValue);

  int GetInt(string key, int defaultValue);
}
