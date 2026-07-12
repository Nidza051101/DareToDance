using DareToDance.Application.Services.Authentication.Jwt;
using DareToDance.Infrastructure.Authentication;
using DareToDance.Infrastructure.UnitTests.TestUtils;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DareToDance.Infrastructure.UnitTests.Authentication;

public class JwtTokenGeneratorTests
{
    private readonly FakeDateTimeProvider _clock = new();
    private readonly JwtTokenGenerator _generator;

    public JwtTokenGeneratorTests()
    {
        _generator = new JwtTokenGenerator(_clock, Options.Create(new JwtSettings
        {
            Secret = new string('k', 64),
            ExpiryMinutes = 60,
            Issuer = "TestIssuer",
            Audience = "TestAudience",
        }));
    }

    private static JsonWebToken Decode(string token) => new JsonWebTokenHandler().ReadJsonWebToken(token);

    [Fact]
    public void GenerateToken_ContainsTheExpectedClaims()
    {
        var subject = new TokenSubject(Guid.NewGuid(), "Nikola", "Andric", "nikola@test.com", Roles: []);

        var jwt = Decode(_generator.GenerateToken(subject));

        Assert.Equal(subject.Id.ToString(), jwt.GetClaim("sub").Value);
        Assert.Equal("nikola@test.com", jwt.GetClaim("email").Value);
        Assert.Equal("Nikola", jwt.GetClaim("given_name").Value);
        Assert.Equal("Andric", jwt.GetClaim("family_name").Value);
        Assert.NotEqual(Guid.Empty, Guid.Parse(jwt.GetClaim("jti").Value));
        Assert.Equal("TestIssuer", jwt.Issuer);
        Assert.Contains("TestAudience", jwt.Audiences);
    }

    [Fact]
    public void GenerateToken_SetsLifetimeFromClockAndSettings()
    {
        var subject = new TokenSubject(Guid.NewGuid(), "Nikola", "Andric", "nikola@test.com", Roles: []);

        var jwt = Decode(_generator.GenerateToken(subject));

        Assert.Equal(_clock.UtcNow, jwt.IssuedAt);
        Assert.Equal(_clock.UtcNow, jwt.ValidFrom);
        Assert.Equal(_clock.UtcNow.AddMinutes(60), jwt.ValidTo);
    }

    [Fact]
    public void GenerateToken_EmitsOneRoleClaimPerRole()
    {
        var subject = new TokenSubject(Guid.NewGuid(), "Nikola", "Andric", "nikola@test.com", ["Admin", "Manager"]);

        var jwt = Decode(_generator.GenerateToken(subject));
        var roles = jwt.Claims.Where(claim => claim.Type == "role").Select(claim => claim.Value).ToList();

        Assert.Equal(["Admin", "Manager"], roles);
    }
}
