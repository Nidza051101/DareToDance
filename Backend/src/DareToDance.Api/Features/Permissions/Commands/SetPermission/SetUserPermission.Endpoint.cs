using DareToDance.Api.Features.Permissions.Shared;
using DareToDance.Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Permissions.Commands.SetUserPermission;

public sealed record SetUserPermissionRequest(Guid UserId, Guid PermissionId, bool IsGranted)
{
    public override string ToString() => $"SetUserPermissionRequest {{ UserId = {UserId}, PermissionId = {PermissionId}, IsGranted = {IsGranted} }}";
}

public sealed class SetUserPermissionEndpoint : PermissionsEndpointBase
{
    [HttpPost("set-permission")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Handle([FromBody] SetUserPermissionRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest();

        var result = await Sender.Send(request.ToCommand(), cancellationToken);

        return result.Match<IActionResult>(_ => Ok(), Problem);
    }
}