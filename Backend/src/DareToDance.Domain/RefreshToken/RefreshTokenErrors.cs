using ErrorOr;

namespace DareToDance.Domain.RefreshToken;

// All Unexpected on purpose: these are invariant violations — handlers check
// state before mutating, so a tripped guard means a bug, and Unexpected maps
// to a 500 instead of masquerading as a business outcome.
public static class RefreshTokenErrors
{
    public static readonly Error TokenHashRequired = Error.Unexpected(
        code: "RefreshToken.TokenHashRequired",
        description: "The token hash is required.");

    public static readonly Error SlidingLifetimeNotPositive = Error.Unexpected(
        code: "RefreshToken.SlidingLifetimeNotPositive",
        description: "The sliding lifetime must be positive.");

    public static readonly Error AbsoluteExpiryNotInFuture = Error.Unexpected(
        code: "RefreshToken.AbsoluteExpiryNotInFuture",
        description: "The absolute expiry must lie in the future.");

    public static readonly Error AlreadyFinalized = Error.Unexpected(
        code: "RefreshToken.AlreadyFinalized",
        description: "The token has already been consumed or revoked.");

    public static readonly Error Expired = Error.Unexpected(
        code: "RefreshToken.Expired",
        description: "Cannot consume an expired token.");
}
