using DareToDance.Domain.RefreshToken;
using DareToDance.Domain.User;
using DareToDance.Domain.User.Id;
using ErrorOr;

namespace DareToDance.Infrastructure.Services;

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);

    // Starts a new session family (login). The entity is NOT persisted here —
    // the caller adds it to the unit of work that commits the login.
    // An error means a tripped domain invariant (a bug), never a business
    // outcome — callers propagate it instead of mapping it to a 401.
    ErrorOr<IssuedRefreshToken> CreateRefreshToken(UserId userId, DateTime utcNow);

    // Consumes the predecessor and returns its successor in the same family,
    // carrying the family's absolute expiry forward. The caller persists both
    // in one SaveChanges so rotation is atomic.
    ErrorOr<IssuedRefreshToken> RotateRefreshToken(RefreshToken predecessor, DateTime utcNow);
}

public sealed record AccessToken(string Token, DateTime ExpiresAtUtc)
{
    public override string ToString()
        => $"AccessToken {{ Token = [REDACTED], ExpiresAtUtc = {ExpiresAtUtc:O} }}";
}

public sealed record IssuedRefreshToken(RefreshToken Token, string WireToken)
{
    public override string ToString()
        => $"IssuedRefreshToken {{ TokenId = {Token.Id.Value}, WireToken = [REDACTED] }}";
}
