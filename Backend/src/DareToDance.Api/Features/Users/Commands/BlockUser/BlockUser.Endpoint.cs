using DareToDance.Api.Features.Users.Shared;
using DareToDance.Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Users.Commands.BlockUser;

public sealed class BlockUserEndpoint : UsersEndpointBase
{
    [HttpPost("{id:guid}/block")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Handle(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(id.ToCommand(), cancellationToken);

        return result.Match<IActionResult>(
            user => Ok(user.ToResponse()),
            Problem);
    }
}
