using DareToDance.Domain.Common;
using DareToDance.Domain.User.Id;
using DareToDance.Domain.Group.Id;
using UserEntity = DareToDance.Domain.User.User;

namespace DareToDance.Domain.Group;

public sealed class Group : Entity<GroupId>
{
    //TODO: isto kao u UserPermissions
    public UserId TeacherId { get; private set; }
    public UserEntity Teacher { get; private set; } = null!;
    public string Name { get; private set; }
    public string DanceStyle { get; private set; } //TODO: mozda razmilsiti da bude entitet?
    public string Level { get; private set; } //TODO: enum
    public string DayOfWeek { get; private set; }   //
    public TimeOnly StartTime { get; private set; } // TODO: mozda ValueObject ili Entitet mozda da se cuva kao tabela
    public TimeOnly EndTime { get; private set; }   //
    public int MaxCapacity { get; private set; } // TODO: mozda da se capacity veze za Dance Hall tj prostor u kom se odrzava cas
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
