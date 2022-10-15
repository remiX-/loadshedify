namespace Proxy.Core.Services;

public interface ITimerService
{
  void Start(string identifier);

  TimeSpan End(string identifier);
}
