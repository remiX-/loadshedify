namespace Proxy.Core.Utils;

public static class UrlHelpers
{
  private const string SeparateChar = "/";

  public static string SafeUrl(params string[] args)
  {
    return args.Aggregate("", (current, next) =>
    {
      if (current.Length == 0)
        return next;

      if (current.EndsWith(SeparateChar))
        return $"{current}{next}";

      return $"{current}{SeparateChar}{next}";
    });
  }
}
