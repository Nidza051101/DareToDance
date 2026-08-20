namespace DareToDance.Api.Features.Users.Commands.CreateUser;

public static partial class CreateUser
{
    public static Command ToCommand(this CreateUserRequest request)
    {
        return new Command(
            request.Email,
            request.FirstName,
            request.LastName,
            request.Password,
            request.Phone);
    }
}
