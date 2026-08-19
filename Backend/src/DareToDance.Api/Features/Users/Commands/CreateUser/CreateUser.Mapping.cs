namespace DareToDance.Api.Features.Users.Commands.CreateUser;

public static class CreateUserMapping
{
    public static CreateUserCommand ToCommand(this CreateUserRequest request)
    {
        return new CreateUserCommand(
            request.Email,
            request.FirstName,
            request.LastName,
            request.Password,
            request.Phone);
    }
}
