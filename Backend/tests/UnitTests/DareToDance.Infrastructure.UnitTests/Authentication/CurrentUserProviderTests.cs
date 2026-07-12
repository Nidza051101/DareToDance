using System.Security.Claims;
using DareToDance.Infrastructure.Authentication;
using Microsoft.AspNetCore.Http;

namespace DareToDance.Infrastructure.UnitTests.Authentication;

public class CurrentUserProviderTests
{
    private static CurrentUserProvider ProviderWith(ClaimsPrincipal? principal)
    {
        var accessor = new HttpContextAccessor();
        if (principal is not null)
        {
            accessor.HttpContext = new DefaultHttpContext { User = principal };
        }

        return new CurrentUserProvider(accessor);
    }

    private static ClaimsPrincipal AuthenticatedPrincipal(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, authenticationType: "Test"));

    [Fact]
    public void GetCurrentUser_WithoutHttpContext_ReturnsNull()
    {
        var provider = ProviderWith(principal: null);

        Assert.Null(provider.GetCurrentUser());
    }

    [Fact]
    public void GetCurrentUser_UnauthenticatedPrincipal_ReturnsNull()
    {
        var provider = ProviderWith(new ClaimsPrincipal(new ClaimsIdentity()));

        Assert.Null(provider.GetCurrentUser());
    }

    [Fact]
    public void GetCurrentUser_AuthenticatedPrincipal_MapsAllClaims()
    {
        var userId = Guid.NewGuid();
        var provider = ProviderWith(AuthenticatedPrincipal(
            new Claim("sub", userId.ToString()),
            new Claim("email", "nikola@test.com"),
            new Claim("given_name", "Nikola"),
            new Claim("family_name", "Andric"),
            new Claim("role", "Admin"),
            new Claim("role", "Manager")));

        var user = provider.GetCurrentUser();

        Assert.NotNull(user);
        Assert.Equal(userId, user.Id);
        Assert.Equal("nikola@test.com", user.Email);
        Assert.Equal("Nikola", user.FirstName);
        Assert.Equal("Andric", user.LastName);
        Assert.Equal(["Admin", "Manager"], user.Roles);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    public void GetCurrentUser_MissingOrMalformedSubClaim_ReturnsNull(string? sub)
    {
        var claims = sub is null ? Array.Empty<Claim>() : [new Claim("sub", sub)];
        var provider = ProviderWith(AuthenticatedPrincipal(claims));

        Assert.Null(provider.GetCurrentUser());
    }
}
