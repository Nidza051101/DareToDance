using DareToDance.Api.Authentication.Contracts;
using DareToDance.Api.Common;
using DareToDance.Application.Services.Authentication;
using DareToDance.Application.Services.Authentication.Commands.Register;
using DareToDance.Application.Services.Authentication.Queries.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Authentication;

[Route("auth")]
public class AuthenticationController(ISender mediator) : ApiController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var command = new RegisterCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password);

        var result = await mediator.Send(command);

        return result.Match(authResult => Ok(MapToResponse(authResult)), Problem);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var query = new LoginQuery(
            request.Email,
            request.Password);

        var result = await mediator.Send(query);

        return result.Match(authResult => Ok(MapToResponse(authResult)), Problem);
    }

    private static AuthenticationResponse MapToResponse(AuthenticationResult result) =>
        new(
            result.Id,
            result.FirstName,
            result.LastName,
            result.Email,
            result.Token);
}
