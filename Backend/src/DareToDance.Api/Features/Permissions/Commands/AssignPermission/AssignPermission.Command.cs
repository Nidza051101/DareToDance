using DareToDance.Domain.PermissionEntity.Id;
using DareToDance.Domain.User.Id;
using DareToDance.Domain.UserPermission;
using DareToDance.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DareToDance.Api.Features.Permissions.Commands.AssignPermission;

public sealed record AssignPermissionCommand(
    Guid UserId,
    Guid PermissionId) : IRequest<ErrorOr<UserPermission>>;

public sealed class AssignPermissionCommandHandler(AppDbContext dbContext)
    : IRequestHandler<AssignPermissionCommand, ErrorOr<UserPermission>>
{
    public async Task<ErrorOr<UserPermission>> Handle(
        AssignPermissionCommand command,
        CancellationToken cancellationToken)
    {
        var userId = UserId.Create(command.UserId);
        var permissionId = PermissionId.Create(command.PermissionId);

        if (!await dbContext.Users.AnyAsync(
                user => user.Id == userId,
                cancellationToken))
        {
            return Error.NotFound(
                "User.NotFound",
                "User was not found.");
        }

        if (!await dbContext.Permissions.AnyAsync(
                permission => permission.Id == permissionId,
                cancellationToken))
        {
            return Error.NotFound(
                "Permission.NotFound",
                "Permission was not found.");
        }

        if (await dbContext.UserPermissions.AnyAsync(
                userPermission =>
                    userPermission.UserId == userId &&
                    userPermission.PermissionId == permissionId,
                cancellationToken))
        {
            return Error.Conflict(
                "UserPermission.AlreadyAssigned",
                "Permission is already assigned to this user.");
        }

        var userPermission = UserPermission.Create(
            userId,
            permissionId);

        dbContext.UserPermissions.Add(userPermission);

        await dbContext.SaveChangesAsync(cancellationToken);

        return userPermission;
    }
}