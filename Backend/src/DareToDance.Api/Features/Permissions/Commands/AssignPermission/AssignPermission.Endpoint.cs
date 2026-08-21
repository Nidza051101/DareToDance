using DareToDance.Api.Features.Permissions.Shared;
using DareToDance.Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Permissions.Commands.AssignPermission;

public sealed record AssignPermissionRequest(
    Guid UserId,
    Guid PermissionId)
{
    public override string ToString()
        => $"AssignPermissionRequest {{ UserId = {UserId}, PermissionId = {PermissionId} }}";
}

public sealed class AssignPermissionEndpoint : PermissionsEndpointBase
{
    [HttpPost("assign")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Handle(
        [FromBody] AssignPermissionRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var command = request.ToCommand();

        var result = await Sender.Send(
            command,
            cancellationToken);

        return result.Match<IActionResult>(
            userPermission => Ok(userPermission),
            Problem);
    }
}