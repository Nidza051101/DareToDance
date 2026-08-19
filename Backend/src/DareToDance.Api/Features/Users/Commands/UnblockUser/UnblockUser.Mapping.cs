namespace DareToDance.Api.Features.Users.Commands.UnblockUser;

public static class UnblockUserMapping
{
    public static UnblockUserCommand ToCommand(this Guid id)
    {
        return new UnblockUserCommand(id);
    }
}
