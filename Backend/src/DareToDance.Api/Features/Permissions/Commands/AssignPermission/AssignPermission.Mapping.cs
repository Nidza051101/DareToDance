using DareToDance.Api.Features.Permissions.Shared;

namespace DareToDance.Api.Features.Permissions.Commands.AssignPermission;

public static class AssignPermissionMapping
{
    public static AssignPermissionCommand ToCommand(
        this AssignPermissionRequest request)
    {
        return new AssignPermissionCommand(
            request.UserId,
            request.PermissionId);
    }
}