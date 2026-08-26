using DareToDance.Api.Features.Auth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Auth.Commands.GoogleLogin;

public sealed record GoogleLoginRequest(string IdToken)
{
    public override string ToString() => "GoogleLoginRequest { IdToken = [REDACTED] }";
}

// Body for the "account doesn't exist yet" case - just enough for the
// frontend to pre-fill the complete-registration form.
public sealed record GoogleAccountNotFoundResponse(string Email, string FirstName, string LastName);

public sealed class GoogleLoginEndpoint : AuthEndpointBase
{
    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<IActionResult> Handle(GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GoogleLogin.Command(request.IdToken), cancellationToken);

        return result.Match<IActionResult>(
            outcome => outcome switch
            {
                GoogleLogin.LoggedIn loggedIn => Ok(loggedIn.Result.ToResponse()),
                GoogleLogin.AccountNotFound notFound => NotFound(
                    new GoogleAccountNotFoundResponse(notFound.Email, notFound.FirstName, notFound.LastName)),
                _ => Problem(),
            },
            Problem);
    }
}
