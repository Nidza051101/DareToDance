namespace DareToDance.Api.Features.Users.Commands.BlockUser;

public static partial class BlockUser
{
    public static Command ToCommand(this Guid id)
    {
        return new Command(id);
    }
}
