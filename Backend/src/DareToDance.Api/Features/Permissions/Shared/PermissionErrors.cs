using ErrorOr;

namespace DareToDance.Api.Features.Permissions.Shared;

public static class PermissionErrors
{
    public static readonly Error DuplicateName = Error.Conflict(
        code: "User.DuplicateName",
        description: "A permission with this name already exists.");
}
