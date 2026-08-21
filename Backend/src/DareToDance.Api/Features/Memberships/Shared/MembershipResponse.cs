namespace DareToDance.Api.Features.Memberships.Shared;

public sealed record MembershipResponse(
    Guid Id,
    Guid UserId,
    DateTime ValidFrom,
    DateTime ValidTo,
    string Status);
