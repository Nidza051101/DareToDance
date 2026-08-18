using DareToDance.Domain.Common;
using DareToDance.Permission.User.Id;

namespace DareToDance.Domain.PermissionEntity;

public sealed class Permission : Entity<PermissionId> //TODO: 10 od 10 svidja mi se
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    private Permission(
        PermissionId id,
        string name,
        string description)
        : base(id)
    {
        Name = name;
        Description = description;
    }

    public static Permission Create(string name, string description)
    {
        return new Permission(
            PermissionId.CreateUnique(),
            name.Trim(),
            description.Trim());
    }
}
