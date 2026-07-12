using System.Security.Claims;
using System.Text;
using DareToDance.Application.Common.Services;
using DareToDance.Application.Services.Authentication.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace DareToDance.Infrastructure.Authentication;

public class JwtTokenGenerator(
    IDateTimeProvider dateTimeProvider,
    IOptions<JwtSettings> jwtOptions) : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public string GenerateToken(TokenSubject subject)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            SecurityAlgorithms.HmacSha256
        );

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, subject.Email),
            new(JwtRegisteredClaimNames.GivenName, subject.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, subject.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(subject.Roles.Select(role => new Claim("role", role)));

        var now = dateTimeProvider.UtcNow;

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(_jwtSettings.ExpiryMinutes),
            SigningCredentials = signingCredentials,
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
