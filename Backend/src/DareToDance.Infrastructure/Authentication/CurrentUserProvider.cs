using DareToDance.Application.Common.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DareToDance.Infrastructure.Authentication;

public class CurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public CurrentUser? GetCurrentUser()
    {
        var principal = httpContextAccessor.HttpContext?.User;

        if (principal?.Identity?.IsAuthenticated is not true ||
            !Guid.TryParse(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var userId))
        {
            return null;
        }

        return new CurrentUser(
            userId,
            principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? string.Empty,
            principal.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value ?? string.Empty,
            principal.FindFirst(JwtRegisteredClaimNames.FamilyName)?.Value ?? string.Empty,
            principal.FindAll("role").Select(claim => claim.Value).ToList());
    }
}
