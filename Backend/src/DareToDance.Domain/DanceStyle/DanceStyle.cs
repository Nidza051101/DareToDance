using DareToDance.Domain.DanceStyle.Id;

using DareToDance.Domain.Common;

namespace DareToDance.Domain.DanceStyle;

public sealed class DanceStyle : AggregateRoot<DanceStyleId>
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    private DanceStyle(
        DanceStyleId id,
        string name,
        string description,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
        : base(id, createdAtUtc, updatedAtUtc)
    {
        Name = name;
        Description = description;
    }

    public static DanceStyle Create(string name, string description)
    {
        var utcNow = DateTime.UtcNow;

        return new DanceStyle(
            DanceStyleId.CreateUnique(),
            name.Trim(),
            description.Trim(),
            utcNow,
            utcNow);
    }
}