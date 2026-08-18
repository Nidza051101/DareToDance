using DareToDance.Domain.Common;
using DareToDance.Domain.PermissionEntity.Id;

namespace DareToDance.Domain.PermissionEntity;

public sealed class Permission : AggregateRoot<PermissionId> 
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    private Permission(
        PermissionId id,
        string name,
        string description,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
        : base(id, createdAtUtc, updatedAtUtc)
    {
        Name = name;
        Description = description;
    }

    public static Permission Create(string name, string description)
    {
        var utcNow = DateTime.UtcNow;

        return new Permission(
            PermissionId.CreateUnique(),
            name.Trim(),
            description.Trim(),
            utcNow,
            utcNow);
    }
}
