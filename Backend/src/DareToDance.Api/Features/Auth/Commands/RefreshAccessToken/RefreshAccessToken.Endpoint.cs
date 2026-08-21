using DareToDance.Api.Features.Auth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Auth.Commands.RefreshAccessToken;

public sealed record RefreshAccessTokenRequest(string RefreshToken);

public sealed class RefreshAccessTokenEndpoint : AuthEndpointBase
{
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Handle(RefreshAccessTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request.ToCommand(), cancellationToken);

        return result.Match<IActionResult>(
            tokens => Ok(tokens.ToResponse()),
            Problem);
    }
}
