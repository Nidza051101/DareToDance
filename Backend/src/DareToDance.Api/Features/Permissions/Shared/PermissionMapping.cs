using DareToDance.Domain.PermissionEntity;

namespace DareToDance.Api.Features.Permissions.Shared;

public static class PermissionMapping
{
    public static PermissionResponse ToResponse(this Permission permission)
    {
        return new PermissionResponse(
            permission.Id.Value,
            permission.Name,
            permission.Description,
            permission.CreatedAtUtc);
    }
}