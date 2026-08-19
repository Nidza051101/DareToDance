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
    public GroupLevel Level { get; private set; }
    public GroupSchedule Schedule { get; private set; }
    public int MaxCapacity { get; private set; } // TODO: mozda da se capacity veze za Dance Hall tj prostor u kom se odrzava cas

    private Group(
        GroupId id,
        UserId teacherId,
        string name,
        DanceStyleId danceStyleId,
        GroupLevel level,
        GroupSchedule schedule,
        int maxCapacity,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
        : base(id, createdAtUtc, updatedAtUtc)
    {
        TeacherId = teacherId;
        Name = name;
        DanceStyleId = danceStyleId;
        Level = level;
        Schedule = schedule;
        MaxCapacity = maxCapacity;
    }

    public static Group Create(
        UserId teacherId,
        string name,
        DanceStyleId danceStyleId,
        GroupLevel level,
        GroupSchedule schedule,
        int maxCapacity)
    {
        var utcNow = DateTime.UtcNow;

        return new Group(
            GroupId.CreateUnique(),
            teacherId,
            name.Trim(),
            danceStyleId,
            level,
            schedule,
            maxCapacity,
            utcNow,
            utcNow);
    }
}
