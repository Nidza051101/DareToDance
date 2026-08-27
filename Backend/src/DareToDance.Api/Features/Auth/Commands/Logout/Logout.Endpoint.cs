using DareToDance.Api.Features.Auth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DareToDance.Api.Features.Auth.Commands.Logout;

public sealed class LogoutEndpoint : AuthEndpointBase
{
    // AllowAnonymous: signing out must work after the access token has already
    // expired — the refresh token itself is the credential here.
    [HttpPost("logout")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-refresh")]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refreshToken"] ?? string.Empty;

        var result = await Sender.Send(new Logout.Command(refreshToken), cancellationToken);

        return result.Match<IActionResult>(
            _ =>
            {
                // Path mora da se poklapa sa onim iz SetRefreshTokenCookie,
                // inače browser ovo tretira kao potpuno drugi kolačić i
                // originalni ostaje živ.
                Response.Cookies.Delete("refreshToken", new CookieOptions { Path = "/" });
                return NoContent();
            },
            Problem);
    }
}
