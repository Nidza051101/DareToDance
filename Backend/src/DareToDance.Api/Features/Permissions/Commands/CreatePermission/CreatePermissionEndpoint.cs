using DareToDance.Api.Features.Permissions.Commands.CreatePermission;
using DareToDance.Api.Features.Permissions.Shared;
using DareToDance.Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public sealed record CreatePermissionRequest(
    string Name,
    string Description)
{
    public override string ToString()
        => $"CreatePermissionRequest {{ Name = {Name}, Description = {Description} }}";
}

public sealed class CreatePermissionEndpoint : PermissionsEndpointBase
{
    [HttpPost("create")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Handle(
        [FromBody] CreatePermissionRequest request,
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
            permission => Created(
                $"/permissions/{permission.Id.Value}",
                permission.ToResponse()),
            Problem);
    }
}