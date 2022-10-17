using System;
using Humanizer;
using Proxy.ESP.Api.Entity;

namespace Proxy.ESP.Api.Extensions;

public static class AreaScheduleEventExtensions
{
  public static string PrettyTimeToStart(this AreaScheduleEvent scheduleEvent, DateTimeOffset baseTime)
  {
    if (scheduleEvent.HasStarted) throw new InvalidOperationException("Event has started already.");

    var timeToStart = scheduleEvent.Start - baseTime;
    return GetFormatted(timeToStart);
  }

  public static string PrettyTimeToEnd(this AreaScheduleEvent scheduleEvent, DateTimeOffset baseTime)
  {
    if (!scheduleEvent.HasStarted) throw new InvalidOperationException("Event has not started or has already ended.");

    var timeToEnd = scheduleEvent.End - baseTime;
    return GetFormatted(timeToEnd);
  }

  private static string GetFormatted(TimeSpan timeSpan)
  {
    return timeSpan.Hours > 0
      ? $"{"hour".ToQuantity(timeSpan.Hours)}, {"min".ToQuantity(timeSpan.Minutes)}"
      : $"{"min".ToQuantity(timeSpan.Minutes)}";
  }
}
