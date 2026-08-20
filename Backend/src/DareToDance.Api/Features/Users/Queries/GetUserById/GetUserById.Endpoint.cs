using DareToDance.Api.Features.Users.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdEndpoint : UsersEndpointBase
{
    [HttpGet("{id:guid}", Name = "GetUserById")]
    [AllowAnonymous]
    public async Task<IActionResult> Handle(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetUserById.Query(id), cancellationToken);

        return result.Match(
            user => Ok(user.ToResponse()),
            Problem);
    }
}
