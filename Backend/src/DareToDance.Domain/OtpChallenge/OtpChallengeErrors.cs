using ErrorOr;

namespace DareToDance.Domain.OtpChallenge;

// All Unexpected on purpose: these are invariant violations — handlers check
// state before mutating, so a tripped guard means a bug, and Unexpected maps
// to a 500 instead of masquerading as a business outcome.
public static class OtpChallengeErrors
{
    public static readonly Error CodeHashRequired = Error.Unexpected(
        code: "OtpChallenge.CodeHashRequired",
        description: "The code hash is required.");

    public static readonly Error LifetimeNotPositive = Error.Unexpected(
        code: "OtpChallenge.LifetimeNotPositive",
        description: "The lifetime must be positive.");

    public static readonly Error AlreadyFinalized = Error.Unexpected(
        code: "OtpChallenge.AlreadyFinalized",
        description: "The challenge has already been consumed or invalidated.");

    public static readonly Error Expired = Error.Unexpected(
        code: "OtpChallenge.Expired",
        description: "Cannot consume an expired challenge.");
}
