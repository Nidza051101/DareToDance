using DareToDance.Domain.PermissionEntity.Id;
using DareToDance.Domain.User.Id;
using DareToDance.Domain.UserPermission;
using DareToDance.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DareToDance.Api.Features.Permissions.Commands.SetUserPermission;

public sealed record SetUserPermissionCommand(Guid UserId, Guid PermissionId, bool IsGranted) : IRequest<ErrorOr<Success>>;

public sealed class SetUserPermissionCommandHandler(AppDbContext dbContext) : IRequestHandler<SetUserPermissionCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(SetUserPermissionCommand command, CancellationToken cancellationToken)
    {
        var userId = UserId.Create(command.UserId);
        var permissionId = PermissionId.Create(command.PermissionId);

        if (!await dbContext.Users.AnyAsync(user => user.Id == userId, cancellationToken))
            return Error.NotFound("User.NotFound", "User was not found.");

        if (!await dbContext.Permissions.AnyAsync(permission => permission.Id == permissionId, cancellationToken))
            return Error.NotFound("Permission.NotFound", "Permission was not found.");

        if (command.IsGranted)
        {
            if (await dbContext.UserPermissions.AnyAsync(up => up.UserId == userId && up.PermissionId == permissionId, cancellationToken))
                return Error.Conflict("UserPermission.AlreadyAssigned", "Permission is already assigned to this user.");

            dbContext.UserPermissions.Add(UserPermission.Create(userId, permissionId));
        }
        else
        {
            var userPermission = await dbContext.UserPermissions.FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId, cancellationToken);

            if (userPermission is null)
                return Error.NotFound("UserPermission.NotFound", "Permission is not assigned to this user.");

            dbContext.UserPermissions.Remove(userPermission);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new Success();
    }
}