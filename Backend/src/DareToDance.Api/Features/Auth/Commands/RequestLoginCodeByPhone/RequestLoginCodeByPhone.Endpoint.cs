using DareToDance.Api.Features.Auth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Auth.Commands.RequestLoginCodeByPhone;

public sealed record RequestLoginCodeByPhoneRequest(string Phone);

public sealed class RequestLoginCodeByPhoneEndpoint : AuthEndpointBase
{
    [HttpPost("login/phone")]
    [AllowAnonymous]
    public async Task<IActionResult> Handle(RequestLoginCodeByPhoneRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request.ToCommand(), cancellationToken);

        return result.Match<IActionResult>(
            _ => Ok(new LoginCodeRequestedResponse("Ako nalog postoji, kod je poslat na uneti broj telefona.")),
            Problem);
    }
}
