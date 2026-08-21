using DareToDance.Api.Features.Permissions.Shared;
using DareToDance.Domain.PermissionEntity;
using DareToDance.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DareToDance.Api.Features.Permissions.Commands.CreatePermission;

public sealed record CreatePermissionCommand(
    string Name,
    string Description) : IRequest<ErrorOr<Permission>>
{
    public override string ToString()
    => $"CreatePermissionCommand {{ Name = {Name}, Description = {Description} }}";
}

public sealed class CreatePermissionCommandHandler(AppDbContext dbContext)
    : IRequestHandler<CreatePermissionCommand, ErrorOr<Permission>>
{
    public async Task<ErrorOr<Permission>> Handle(
        CreatePermissionCommand command,
        CancellationToken cancellationToken)
    {
        var name = command.Name.Trim();

        if (await dbContext.Permissions.AnyAsync(
                u => u.Name == name,
                cancellationToken))
        {
            return PermissionErrors.DuplicateName;
        }

        var permission = Permission.Create(
           name,
           command.Description);

        dbContext.Permissions.Add(permission);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException e) when (
            e.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "ix_permissions_name"
            })
        {
            return PermissionErrors.DuplicateName;
        }

        return permission;
    }
}