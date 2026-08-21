using ErrorOr;

namespace DareToDance.Api.Features.Memberships.Shared;

public static class MembershipErrors
{
    public static readonly Error UserNotFound = Error.NotFound(
        code: "Membership.UserNotFound",
        description: "User with the specified id was not found.");
}
