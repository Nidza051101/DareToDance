using DareToDance.Api.Features.Auth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Auth.Commands.CompleteGoogleRegistration;

public sealed record CompleteGoogleRegistrationRequest(string IdToken, string Phone)
{
    public override string ToString()
        => "CompleteGoogleRegistrationRequest { IdToken = [REDACTED], Phone = [REDACTED] }";
}

public sealed class CompleteGoogleRegistrationEndpoint : AuthEndpointBase
{
    [HttpPost("google/complete-registration")]
    [AllowAnonymous]
    public async Task<IActionResult> Handle(
        CompleteGoogleRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new CompleteGoogleRegistration.Command(request.IdToken, request.Phone),
            cancellationToken);

        return result.Match<IActionResult>(
            auth =>
            {
                SetRefreshTokenCookie(auth.RefreshToken, auth.RefreshTokenExpiresAtUtc);
                return Ok(auth.ToResponse());
            },
            Problem);
    }
}
