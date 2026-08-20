using DareToDance.Api.Features.Auth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Auth.Commands.VerifyLoginCode;

public sealed record VerifyLoginCodeRequest(string Recipient, string Code);

public sealed record AccessTokenResponse(string AccessToken, DateTime ExpiresAtUtc);

public sealed class VerifyLoginCodeEndpoint : AuthEndpointBase
{
    [HttpPost("login/verify")]
    [AllowAnonymous]
    public async Task<IActionResult> Handle(VerifyLoginCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request.ToCommand(), cancellationToken);

        return result.Match<IActionResult>(
            login => Ok(login.ToResponse()),
            Problem);
    }
}
