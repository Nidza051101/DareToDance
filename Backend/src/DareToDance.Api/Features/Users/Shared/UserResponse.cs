namespace DareToDance.Api.Features.Users.Shared;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    DateTime CreatedAtUtc);
