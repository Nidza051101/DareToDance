namespace DareToDance.Api.Features.Users.Commands.RegisterUser;

public static class RegisterUserMapping
{
    public static RegisterUserCommand ToCommand(this RegisterUserRequest request)
    {
        return new RegisterUserCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone);
    }
}