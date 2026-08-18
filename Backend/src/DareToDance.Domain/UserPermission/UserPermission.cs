using DareToDance.Domain.Common;
using DareToDance.Domain.User;
using DareToDance.Domain.User.Id;
using DareToDance.Domain.Permission;
using DareToDance.Domain.Permission.Id;

namespace DareToDance.Domain.UserPermission;

public sealed class UserPermission : Entity<UserPermissionId>
{
    public UserId UserId { get; private set; }
    public User User { get; private set; } = null!;
    public PermissionId PermissionId { get; private set; }
    public Permission Permission { get; private set; } = null!;

    private UserPermission(
        UserPermissionId id,
        UserId userId,
        PermissionId permissionId)
        : base(id)
    {
        UserId = userId;
        PermissionId = permissionId;
    }

    public static UserPermission Create(UserId userId, PermissionId permissionId)
    {
        return new UserPermission(
            UserPermissionId.CreateUnique(),
            userId,
            permissionId);
    }
}
