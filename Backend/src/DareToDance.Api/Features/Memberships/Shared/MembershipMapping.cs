using DareToDance.Domain.Membership;

namespace DareToDance.Api.Features.Memberships.Shared;

public static class MembershipMapping
{
    public static MembershipResponse ToResponse(this Membership membership)
    {
        return new MembershipResponse(
            membership.Id.Value,
            membership.UserId.Value,
            membership.ValidFrom,
            membership.ValidTo,
            membership.Status.ToString());
    }
}
