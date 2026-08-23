using DareToDance.Api.Features.Auth.Shared;
using DareToDance.Infrastructure.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace DareToDance.Api.Features.Auth.Commands.RequestOtp;

public sealed record RequestOtpRequest(string Email);

public sealed record RequestOtpResponse(string Message, int ResendCooldownSeconds);

public sealed class RequestOtpEndpoint : AuthEndpointBase
{
    [HttpPost("otp/request")]
    [AllowAnonymous]
    [EnableRateLimiting("otp-request")]
    public async Task<IActionResult> Handle(
        RequestOtpRequest request,
        [FromServices] IOptions<OtpSettings> otpOptions,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new RequestOtp.Command(request.Email), cancellationToken);

        // Byte-identical 202 for every non-validation outcome — unknown email,
        // cooldown, daily cap, and success must be indistinguishable.
        return result.Match<IActionResult>(
            _ => Accepted(new RequestOtpResponse(
                "If an account exists for this email, a sign-in code has been sent.",
                otpOptions.Value.ResendCooldownSeconds)),
            Problem);
    }
}
