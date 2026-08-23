using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DareToDance.Infrastructure.Services;

internal sealed class TokenService(
    IOptions<JwtSettings> jwtOptions,
    TimeProvider timeProvider) : ITokenService
{
    public AccessToken CreateAccessToken(User user)
    {
        var settings = jwtOptions.Value;
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var expiresAtUtc = utcNow.AddMinutes(settings.ExpiryMinutes);

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret)),
            SecurityAlgorithms.HmacSha256);

        // Minimal claim set on purpose: no email, name, or phone — tokens end
        // up in Authorization headers, proxy logs, and traces.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                EpochTime.GetIntDate(utcNow).ToString(),
                ClaimValueTypes.Integer64)
        };

        // No nbf claim on purpose: exp bounds the lifetime, and a not-before
        // stamped by this instance's clock only causes spurious rejections
        // when validator clocks (or a test clock) run behind the issuer.
        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: null,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
