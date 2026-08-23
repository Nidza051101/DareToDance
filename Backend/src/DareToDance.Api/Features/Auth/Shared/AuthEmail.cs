namespace DareToDance.Api.Features.Auth.Shared;

public static class AuthEmail
{
    // Must match User.Create's normalization exactly — otherwise lookups for
    // real users silently miss and every request takes the unknown-email path.
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
