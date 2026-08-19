using DareToDance.Domain.Common;

namespace DareToDance.Domain.Group;

public sealed class GroupSchedule : ValueObject
{
    public string DayOfWeek { get; }
    public TimeOnly StartTime { get; }
    public TimeOnly EndTime { get; }

    private GroupSchedule(string dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
    }

    public static GroupSchedule Create(string dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (string.IsNullOrWhiteSpace(dayOfWeek))
        {
            throw new ArgumentException("The day of the week is necessary.", nameof(dayOfWeek));
        }

        if (endTime <= startTime)
        {
            throw new ArgumentException("The end time must be after the start time.", nameof(endTime));
        }

        return new GroupSchedule(dayOfWeek.Trim(), startTime, endTime);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DayOfWeek;
        yield return StartTime;
        yield return EndTime;
    }
}
