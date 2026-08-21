using DareToDance.Api.Features.Users.Queries.GetUserPermissions;
using DareToDance.Api.Features.Permissions.Shared;
using DareToDance.Api.Features.Users.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DareToDance.Domain.User;

namespace DareToDance.Api.Features.Permissions.Queries.GetUserPermissionById;

public sealed class GetPermissionByIdEndpoint : UsersEndpointBase
{
    [HttpGet("{id}/permissions", Name = "GetUserPermissions")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Handle(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetUserPermissionsQuery.Query(id), cancellationToken);

        return result.Match(
        permissions => Ok(permissions.ToResponse()),
        Problem);
    }
}
