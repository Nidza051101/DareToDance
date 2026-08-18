using DareToDance.Domain.Common;
using DareToDance.Domain.User;
using DareToDance.Domain.User.Id;
using DareToDance.Domain.Group.Id;

namespace DareToDance.Domain.Group;

public sealed class Group : Entity<GroupId>
{
    public UserId TeacherId { get; private set; }
    public User Teacher { get; private set; } = null!;
    public string Name { get; private set; }
    public string DanceStyle { get; private set; }
    public string Level { get; private set; }
    public string DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public int MaxCapacity { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Group(
        GroupId id,
        UserId teacherId,
        string name,
        string danceStyle,
        string level,
        string dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        int maxCapacity,
        DateTime createdAtUtc)
        : base(id)
    {
        TeacherId = teacherId;
        Name = name;
        DanceStyle = danceStyle;
        Level = level;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        MaxCapacity = maxCapacity;
        CreatedAtUtc = createdAtUtc;
    }

    public static Group Create(
        UserId teacherId,
        string name,
        string danceStyle,
        string level,
        string dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        int maxCapacity)
    {
        var utcNow = DateTime.UtcNow;

        return new Group(
            GroupId.CreateUnique(),
            teacherId,
            name.Trim(),
            danceStyle.Trim(),
            level.Trim(),
            dayOfWeek.Trim(),
            startTime,
            endTime,
            maxCapacity,
            utcNow);
    }
}
