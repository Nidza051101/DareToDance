using DareToDance.Api.Features.Users.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Users.CreateUser;

public sealed record CreateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string Password)
{
    public override string ToString()
        =>
            $"CreateUserRequest {{ Email = {Email}, FirstName = {FirstName}, LastName = {LastName}, Password = [REDACTED] }}";
}

public sealed class CreateUserEndpoint : UsersEndpointBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request.ToCommand(), cancellationToken);
        
        return result.Match<IActionResult>(
            user => Created($"/users/{user.Id.Value}", user.ToResponse()),
            Problem);
    }
}