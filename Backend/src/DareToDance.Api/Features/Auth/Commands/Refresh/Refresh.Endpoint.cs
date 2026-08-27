using DareToDance.Api.Features.Auth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DareToDance.Api.Features.Auth.Commands.Refresh;

public sealed record RefreshRequest(string RefreshToken)
{
    public override string ToString()
        => "RefreshRequest { RefreshToken = [REDACTED] }";
}

public sealed class RefreshEndpoint : AuthEndpointBase
{
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-refresh")]
    public async Task<IActionResult> Handle(RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new Refresh.Command(request.RefreshToken), cancellationToken);

        return result.Match<IActionResult>(
            auth =>
            {
                SetRefreshTokenCookie(auth.RefreshToken, auth.RefreshTokenExpiresAtUtc);
                return Ok(auth.ToResponse());
            },
            Problem);
    }
}
