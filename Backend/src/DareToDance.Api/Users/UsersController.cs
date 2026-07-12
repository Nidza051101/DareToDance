using DareToDance.Api.Common;
using DareToDance.Application.Services.Users.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Users;

[Route("users")]
public class UsersController(ISender mediator) : ApiController
{
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var result = await mediator.Send(new GetCurrentUserQuery());

        return result.Match(user => Ok(user), Problem);
    }
}
