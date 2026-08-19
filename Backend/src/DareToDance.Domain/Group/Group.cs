using DareToDance.Domain.Common;
using DareToDance.Domain.User.Id;
using DareToDance.Domain.Group.Id;
using DareToDance.Domain.DanceStyle.Id;

namespace DareToDance.Domain.Group;

public sealed class Group : AggregateRoot<GroupId>
{
    public UserId TeacherId { get; private set; }
    public string Name { get; private set; }
    public DanceStyleId DanceStyleId { get; private set; }
    public string Level { get; private set; } //TODO: enum
    public string DayOfWeek { get; private set; }   //
    public TimeOnly StartTime { get; private set; } // TODO: mozda ValueObject ili Entitet mozda da se cuva kao tabela
    public TimeOnly EndTime { get; private set; }   //
    public int MaxCapacity { get; private set; } // TODO: mozda da se capacity veze za Dance Hall tj prostor u kom se odrzava cas

    private Group(
        GroupId id,
        UserId teacherId,
        string name,
        DanceStyleId danceStyleId,
        string level,
        string dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        int maxCapacity,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
        : base(id, createdAtUtc, updatedAtUtc)
    {
        TeacherId = teacherId;
        Name = name;
        DanceStyleId = danceStyleId;
        Level = level;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        MaxCapacity = maxCapacity;
    }

    public static Group Create(
        UserId teacherId,
        string name,
        DanceStyleId danceStyleId,
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
            danceStyleId,
            level.Trim(),
            dayOfWeek.Trim(),
            startTime,
            endTime,
            maxCapacity,
            utcNow,
            utcNow);
    }
}
