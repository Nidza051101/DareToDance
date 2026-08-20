using ErrorOr;

namespace DareToDance.Api.Features.Auth.Shared;

public static class AuthErrors
{
    public static readonly Error InvalidCode = Error.Validation(
        code: "Auth.InvalidCode",
        description: "The code is invalid or has expired.");

    public static readonly Error CodeAlreadySent = Error.Conflict(
        code: "Auth.CodeAlreadySent",
        description: "A code was already sent. Please wait before requesting a new one.");

    public static readonly Error AccountBlocked = Error.Forbidden(
        code: "Auth.AccountBlocked",
        description: "The account has been blocked after too many failed attempts.");
}
