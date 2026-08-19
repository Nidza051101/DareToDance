using DareToDance.Api.Features.Auth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Auth.Commands.RequestLoginCodeByEmail;

public sealed record RequestLoginCodeByEmailRequest(string Email);

public sealed class RequestLoginCodeByEmailEndpoint : AuthEndpointBase
{
    [HttpPost("login/email")]
    [AllowAnonymous]
    public async Task<IActionResult> Handle(RequestLoginCodeByEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request.ToCommand(), cancellationToken);

        return result.Match<IActionResult>(
            _ => Ok(new LoginCodeRequestedResponse("If an account exists, a code has been sent to the given email.")),
            Problem);
    }
}
