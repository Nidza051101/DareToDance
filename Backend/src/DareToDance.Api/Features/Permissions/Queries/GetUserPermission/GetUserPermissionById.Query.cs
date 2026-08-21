using DareToDance.Domain.PermissionEntity;
using DareToDance.Domain.User.Id;
using DareToDance.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DareToDance.Api.Features.Users.Queries.GetUserPermissions;

public static class GetUserPermissionsQuery
{
    public sealed record Query(Guid UserId) : IRequest<ErrorOr<List<Permission>>>;

    public sealed class Handler(AppDbContext dbContext)
        : IRequestHandler<Query, ErrorOr<List<Permission>>>
    {
        public async Task<ErrorOr<List<Permission>>> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            var userId = UserId.Create(query.UserId);
            
            var userExists = await dbContext.Users
                .AnyAsync(u => u.Id == userId, cancellationToken);

            if (!userExists)
                return Error.NotFound("User.NotFound", "User not found");

            var permissions = await dbContext.UserPermissions
                .Where(up => up.UserId == userId)
                .Join(dbContext.Permissions,
                    up => up.PermissionId,
                    p => p.Id,
                    (up, p) => p)
                .ToListAsync(cancellationToken);

            return permissions;
        }
    }
}