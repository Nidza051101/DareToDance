using DareToDance.Api.Features.Auth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DareToDance.Api.Features.Auth.Commands.VerifyOtp;

public sealed record VerifyOtpRequest(string Email, string Code)
{
    public override string ToString()
        => $"VerifyOtpRequest {{ Email = {Email}, Code = [REDACTED] }}";
}

public sealed class VerifyOtpEndpoint : AuthEndpointBase
{
    [HttpPost("otp/verify")]
    [AllowAnonymous]
    [EnableRateLimiting("otp-verify")]
    public async Task<IActionResult> Handle(VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new VerifyOtp.Command(request.Email, request.Code), cancellationToken);

        return result.Match<IActionResult>(
            auth =>
            {
                SetRefreshTokenCookie(auth.RefreshToken, auth.RefreshTokenExpiresAtUtc);
                return Ok(auth.ToResponse());
            },
            Problem);
    }
}
