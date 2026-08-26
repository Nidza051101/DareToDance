using ErrorOr;

namespace DareToDance.Infrastructure.Services;

public static class GoogleAuthErrors
{
    public static readonly Error InvalidToken = Error.Unauthorized(
        code: "GoogleAuth.InvalidToken",
        description: "The Google token is invalid or has expired.");
}
