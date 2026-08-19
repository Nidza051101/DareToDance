using ErrorOr;

namespace DareToDance.Api.Features.Auth.Shared;

public static class AuthErrors
{
    public static readonly Error InvalidCode = Error.Validation(
        code: "Auth.InvalidCode",
        description: "The code is invalid or has expired.");

    public static readonly Error TooManyAttempts = Error.Validation(
        code: "Auth.TooManyAttempts",
        description: "Too many failed attempts. Request a new code.");

    public static readonly Error CodeAlreadySent = Error.Conflict(
        code: "Auth.CodeAlreadySent",
        description: "A code was already sent. Please wait before requesting a new one.");
}
