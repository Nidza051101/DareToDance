using DareToDance.Api.Features.Users.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Users.Commands.RegisterUser;

public sealed record RegisterUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone)
{
    public override string ToString()
        => $"RegisterUserRequest {{ FirstName = {FirstName}, LastName = {LastName}, Email = {Email}, Phone = {Phone} }}";
}

public sealed class RegisterUserEndpoint : UsersEndpointBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Handle(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var command = new RegisterUserCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone ?? string.Empty);

        var result = await Sender.Send(
            command,
            cancellationToken);

        return result.Match<IActionResult>(
            user => Created(
                $"/users/{user.Id.Value}",
                user.ToResponse()),
            Problem);
    }
}