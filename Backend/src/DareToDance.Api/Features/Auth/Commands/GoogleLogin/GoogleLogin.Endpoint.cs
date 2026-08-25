using DareToDance.Api.Features.Auth.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Auth.Commands.GoogleLogin;

public sealed class GoogleLoginEndpoint(ISender sender) : AuthEndpointBase
{
    [HttpPost("google-login")]
    [AllowAnonymous]
    public async Task<IActionResult> Handle(
        GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new GoogleLogin.Command(request.IdToken);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            authResult => Ok(authResult),
            errors => Problem(errors));
    }
}

