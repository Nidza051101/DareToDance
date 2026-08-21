using DareToDance.Api.Features.Permissions.Shared;
using DareToDance.Domain.PermissionEntity;
using System.Linq;

namespace DareToDance.Api.Features.Permissions.Queries.GetUserPermissionById;

public static class GetUserPermissionsMapping
{
    public static List<PermissionResponse> ToResponse(this List<Permission> permissions)
    {
        return permissions.Select(p => new PermissionResponse(
            p.Id.Value,
            p.Name,
            p.Description,
            p.CreatedAtUtc)).ToList();
    }
}