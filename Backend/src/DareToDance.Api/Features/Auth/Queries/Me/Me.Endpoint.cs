using DareToDance.Api.Features.Auth.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Auth.Queries.Me;

// Smoke endpoint for the JWT pipeline: no [AllowAnonymous], so the inherited
// [Authorize] on ApiEndpointBase must let a valid bearer token through.
// JwtBearer does not remap inbound claims, so the subject claim stays "sub".
public sealed class MeEndpoint : AuthEndpointBase
{
    [HttpGet("me")]
    public IActionResult Handle()
    {
        var sub = User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(sub, out var userId))
        {
            return Unauthorized();
        }

        return Ok(new { UserId = userId });
    }
}
