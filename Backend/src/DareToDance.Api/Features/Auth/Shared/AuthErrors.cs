using ErrorOr;

namespace DareToDance.Api.Features.Auth.Shared;

public static class AuthErrors
{
    public static readonly Error InvalidCode = Error.Validation(
        code: "Auth.InvalidCode",
        description: "Kod nije ispravan ili je istekao.");

    public static readonly Error TooManyAttempts = Error.Validation(
        code: "Auth.TooManyAttempts",
        description: "Prekoracen je broj pokusaja. Zatrazite novi kod.");

    public static readonly Error CodeAlreadySent = Error.Conflict(
        code: "Auth.CodeAlreadySent",
        description: "Kod je vec poslat, sacekajte pre nego sto zatrazite novi.");
}
