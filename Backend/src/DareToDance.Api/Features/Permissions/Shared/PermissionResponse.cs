namespace DareToDance.Api.Features.Permissions.Shared;

public sealed record PermissionResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAtUtc);