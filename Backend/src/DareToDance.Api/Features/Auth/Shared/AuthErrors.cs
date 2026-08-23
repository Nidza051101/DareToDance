using ErrorOr;

namespace DareToDance.Api.Features.Auth.Shared;

public static class AuthErrors
{
    // Deliberately the ONLY error for every verify failure — unknown email,
    // no active challenge, wrong code, expired code, attempt cap reached.
    // Distinct errors on an unauthenticated endpoint would let a caller probe
    // which accounts exist and which have a login attempt in flight.
    public static readonly Error InvalidCode = Error.Unauthorized(
        code: "Auth.InvalidCode",
        description: "The code is invalid or has expired.");
}
