using DareToDance.Api.Features.Memberships.Shared;

namespace DareToDance.Api.Features.Memberships.Commands.CreateMembership;

public static partial class CreateMembership
{
    public static Command ToCommand(this CreateMembershipRequest request)
    {
        return new Command(request.UserId, request.ValidFrom, request.ValidTo);
    }
}
