using DareToDance.Domain.Common;
using DareToDance.Domain.PermissionEntity;
using DareToDance.Domain.PermissionEntity.Id;
using DareToDance.Domain.User.Id;
using DareToDance.Domain.UserPermission.Id;
using UserEntity = DareToDance.Domain.User.User;

namespace DareToDance.Domain.UserPermission;

public sealed class UserPermission : Entity<UserPermissionId>
{
    //TODO: Istrazite ef core mapiranje objetka u tabelu i obrnuto
    public UserId UserId { get; private set; }
    public UserEntity User { get; private set; } = null!;
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
