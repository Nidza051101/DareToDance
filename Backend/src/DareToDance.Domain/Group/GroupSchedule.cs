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
            throw new ArgumentException("Dan u nedelji je obavezan.", nameof(dayOfWeek));
        }

        if (endTime <= startTime)
        {
            throw new ArgumentException("Vreme završetka mora biti posle vremena početka.", nameof(endTime));
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
