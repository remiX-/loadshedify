﻿namespace Proxy.ESP.Api.Entity;

public struct AreaScheduleInfo
{
  public string Name { get; init; }

  public string Region { get; init; }
}

public struct AreaSchedule
{
  public IList<ScheduleDay> Days { get; init; }

  public string Source { get; init; }
}

public struct ScheduleDay
{
  public string Date { get; init; }

  public string Name { get; init; }

  public IList<IList<string>> Stages { get; init; }
}
