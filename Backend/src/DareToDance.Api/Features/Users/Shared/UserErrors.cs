using ErrorOr;

namespace DareToDance.Api.Features.Users.Shared;

public static class UserErrors
{
    public static readonly Error DuplicateEmail = Error.Conflict(
        code: "User.DuplicateEmail",
        description: "A user with this email already exists.");

    public static readonly Error DuplicatePhone = Error.Conflict(
        code: "User.DuplicatePhone",
        description: "A user with this phone already exists.");
    
    public static readonly Error NotFound = Error.NotFound(
        code: "User.NotFound",
        description: "User with the specified id was not found.");
}
