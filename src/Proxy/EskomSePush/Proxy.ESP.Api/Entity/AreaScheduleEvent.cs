using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Proxy.ESP.Api.Entity;

public struct AreaScheduleEvent
{
  public string Note { get; init; }

  public DateTimeOffset Start { get; init; }

  public DateTimeOffset End { get; init; }

  [JsonIgnore]
  public bool HasStarted => Start.UtcDateTime <= DateTime.UtcNow;

  // [JsonIgnore]
  // public TimeSpan TimeToStart => Start.UtcDateTime - DateTime.UtcNow;
  //
  // [JsonIgnore]
  // public TimeSpan TimeToEnd => End.UtcDateTime - DateTime.UtcNow;


  [JsonIgnore]
  public int Stage => int.Parse(Regex.Match(Note, @"stage (\d+)", RegexOptions.IgnoreCase).Groups[1].Value);
}
