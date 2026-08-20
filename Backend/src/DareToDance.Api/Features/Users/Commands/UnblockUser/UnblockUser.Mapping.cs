namespace DareToDance.Api.Features.Users.Commands.UnblockUser;

public static partial class UnblockUser
{
    public static Command ToCommand(this Guid id)
    {
        return new Command(id);
    }
}
