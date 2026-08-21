using DareToDance.Api.Features.Permissions.Shared;
using DareToDance.Domain.PermissionEntity;

namespace DareToDance.Api.Features.Permissions.Commands.CreatePermission;

public static class CreatePermissionMapping
{
    public static CreatePermissionCommand ToCommand(this CreatePermissionRequest request)
    {
        return new CreatePermissionCommand(
            request.Name,
            request.Description);
    }
}