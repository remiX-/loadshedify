namespace Proxy.Core.Extensions;

public static class StringExtensions
{
  public static bool IsNullOrEmpty(this string val)
  {
    return string.IsNullOrWhiteSpace(val);
  }
}
