namespace DareToDance.Api.Features.Users.Commands.BlockUser;

public static class BlockUserMapping
{
    public static BlockUserCommand ToCommand(this Guid id)
    {
        return new BlockUserCommand(id);
    }
}
