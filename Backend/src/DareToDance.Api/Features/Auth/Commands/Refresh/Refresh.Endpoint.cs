using DareToDance.Api.Features.Auth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DareToDance.Api.Features.Auth.Commands.Refresh;

public sealed class RefreshEndpoint : AuthEndpointBase
{
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-refresh")]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refreshToken"] ?? string.Empty;

        var result = await Sender.Send(new Refresh.Command(refreshToken), cancellationToken);

        return result.Match<IActionResult>(
            auth =>
            {
                SetRefreshTokenCookie(auth.RefreshToken, auth.RefreshTokenExpiresAtUtc);
                return Ok(auth.ToResponse());
            },
            Problem);
    }
}
