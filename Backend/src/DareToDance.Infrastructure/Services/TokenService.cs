using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DareToDance.Domain.RefreshToken;
using DareToDance.Domain.RefreshToken.Id;
using DareToDance.Domain.User;
using DareToDance.Domain.User.Id;
using DareToDance.Infrastructure.Options;
using ErrorOr;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DareToDance.Infrastructure.Services;

internal sealed class TokenService(
    IOptions<JwtSettings> jwtOptions,
    IOptions<RefreshTokenSettings> refreshTokenOptions,
    IRefreshTokenHasher refreshTokenHasher,
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

    public ErrorOr<IssuedRefreshToken> CreateRefreshToken(UserId userId, DateTime utcNow)
    {
        return Issue(
            userId,
            familyId: Guid.CreateVersion7(),
            utcNow,
            absoluteExpiresAtUtc: utcNow.AddDays(refreshTokenOptions.Value.AbsoluteLifetimeDays));
    }

    public ErrorOr<IssuedRefreshToken> RotateRefreshToken(RefreshToken predecessor, DateTime utcNow)
    {
        var successor = Issue(
            predecessor.UserId,
            predecessor.FamilyId,
            utcNow,
            predecessor.AbsoluteExpiresAtUtc);

        if (successor.IsError)
        {
            return successor.Errors;
        }

        // Consuming here (not in the handler) keeps the invariant that every
        // consumed row points at its replacement.
        var consumed = predecessor.Consume(utcNow, successor.Value.Token.Id);

        if (consumed.IsError)
        {
            return consumed.Errors;
        }

        return successor;
    }

    private ErrorOr<IssuedRefreshToken> Issue(
        UserId userId,
        Guid familyId,
        DateTime utcNow,
        DateTime absoluteExpiresAtUtc)
    {
        var id = RefreshTokenId.CreateUnique();
        var secret = RefreshTokenWireFormat.GenerateSecret();

        var token = RefreshToken.Create(
            id,
            userId,
            familyId,
            refreshTokenHasher.Hash(id.Value, secret),
            utcNow,
            TimeSpan.FromDays(refreshTokenOptions.Value.SlidingLifetimeDays),
            absoluteExpiresAtUtc);

        if (token.IsError)
        {
            return token.Errors;
        }

        return new IssuedRefreshToken(token.Value, RefreshTokenWireFormat.Format(id, secret));
    }
}
