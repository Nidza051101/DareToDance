using DareToDance.Api.Common.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Auth.Shared;

[Route("auth")]
[Tags("Auth")]
public abstract class AuthEndpointBase : ApiEndpointBase
{
    protected void SetRefreshTokenCookie(string refreshToken, DateTime expiresAtUtc)
    {
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAtUtc,
            Path = "/",
        });
    }
}
