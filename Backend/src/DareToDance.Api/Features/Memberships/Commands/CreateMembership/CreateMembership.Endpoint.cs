using DareToDance.Api.Features.Memberships.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DareToDance.Api.Features.Memberships.Commands.CreateMembership;

public sealed record CreateMembershipRequest(Guid UserId, DateTime ValidFrom, DateTime ValidTo);

public sealed class CreateMembershipEndpoint : MembershipsEndpointBase
{
    [HttpPost]
    public async Task<IActionResult> Handle(CreateMembershipRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request.ToCommand(), cancellationToken);

        return result.Match<IActionResult>(
            membership => Created($"/memberships/{membership.Id.Value}", membership.ToResponse()),
            Problem);
    }
}
