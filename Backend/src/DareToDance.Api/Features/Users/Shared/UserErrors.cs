using ErrorOr;

namespace DareToDance.Api.Features.Users.Shared;

public static class UserErrors
{
    public static readonly Error DuplicateEmail = Error.Conflict(
        code: "User.DuplicateEmail",
        description: "A user with this email already exists.");
}
