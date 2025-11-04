
namespace Fusion.Service.Commons.Background;


public abstract record JobSchedule(bool RunOnStartup = true)
{
    public abstract DateTimeOffset GetNextRun(DateTimeOffset nowUtc, TimeZoneInfo tz);
}

public sealed record IntervalSchedule(TimeSpan Interval, bool RunOnStartup = true)
    : JobSchedule(RunOnStartup)
{
    public override DateTimeOffset GetNextRun(DateTimeOffset nowUtc, TimeZoneInfo tz) => nowUtc + Interval;
}

public sealed record DailyAtSchedule(TimeOnly LocalTime, bool RunOnStartup = false)
    : JobSchedule(RunOnStartup)
{
    public override DateTimeOffset GetNextRun(DateTimeOffset nowUtc, TimeZoneInfo tz)
    {
        var local = TimeZoneInfo.ConvertTime(nowUtc, tz);
        var next = local.Date.Add(LocalTime.ToTimeSpan());
        if (next <= local) next = next.AddDays(1);
        return TimeZoneInfo.ConvertTimeToUtc(next, tz);
    }
}