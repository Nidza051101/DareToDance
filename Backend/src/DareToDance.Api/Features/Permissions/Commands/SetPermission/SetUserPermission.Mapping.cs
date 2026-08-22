namespace DareToDance.Api.Features.Permissions.Commands.SetUserPermission;

public static class SetUserPermissionMapping
{
    public static SetUserPermissionCommand ToCommand(this SetUserPermissionRequest request)
    {
        return new SetUserPermissionCommand(
            request.UserId,
            request.PermissionId,
            request.IsGranted);
    }
}