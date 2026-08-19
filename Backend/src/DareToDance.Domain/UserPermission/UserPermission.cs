using DareToDance.Domain.Common;
using DareToDance.Domain.PermissionEntity.Id;
using DareToDance.Domain.User.Id;
using DareToDance.Domain.UserPermission.Id;

namespace DareToDance.Domain.UserPermission;

public sealed class UserPermission : AggregateRoot<UserPermissionId>
{
    public UserId UserId { get; private set; }
    public PermissionId PermissionId { get; private set; }

    private UserPermission(
        UserPermissionId id,
        UserId userId,
        PermissionId permissionId,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
        : base(id, createdAtUtc, updatedAtUtc)
    {
        UserId = userId;
        PermissionId = permissionId;
    }

    public static UserPermission Create(UserId userId, PermissionId permissionId)
    {
        var utcNow = DateTime.UtcNow;

        return new UserPermission(
            UserPermissionId.CreateUnique(),
            userId,
            permissionId,
            utcNow,
            utcNow);
    }
}
