namespace Proxy.Core.Services;

public class TimerService
{
  private readonly Dictionary<string, DateTime> _idToStartTimeMap;

  public TimerService()
  {
    _idToStartTimeMap = new Dictionary<string, DateTime>();
  }

  public void Start(string identifier)
  {
    if (_idToStartTimeMap.ContainsKey(identifier))
      throw new Exception($"Key '{identifier} already started, call .End to end.");

    _idToStartTimeMap.Add(identifier, DateTime.Now);
  }

  public TimeSpan End(string identifier)
  {
    if (!_idToStartTimeMap.ContainsKey(identifier))
      throw new KeyNotFoundException($"Key '{identifier} not found found, call .Start first.");

    var duration = DateTime.Now - _idToStartTimeMap[identifier];
    _idToStartTimeMap.Remove(identifier);

    return duration;
  }
}
