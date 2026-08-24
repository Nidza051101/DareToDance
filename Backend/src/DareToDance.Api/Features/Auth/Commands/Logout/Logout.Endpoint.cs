using DareToDance.Api.Features.Auth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DareToDance.Api.Features.Auth.Commands.Logout;

public sealed record LogoutRequest(string RefreshToken)
{
    public override string ToString()
        => "LogoutRequest { RefreshToken = [REDACTED] }";
}

public sealed class LogoutEndpoint : AuthEndpointBase
{
    // AllowAnonymous: signing out must work after the access token has already
    // expired — the refresh token itself is the credential here.
    [HttpPost("logout")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-refresh")]
    public async Task<IActionResult> Handle(LogoutRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new Logout.Command(request.RefreshToken), cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            Problem);
    }
}
