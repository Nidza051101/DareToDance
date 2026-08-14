using DareToDance.Api.Features.Users.Shared;
using DareToDance.Domain.User;

namespace DareToDance.Api.Features.Users.CreateUser;

public static class CreateUserMapping
{
    public static CreateUserCommand ToCommand(this CreateUserRequest request)
    {
        return new CreateUserCommand(
            request.Email,
            request.FirstName,
            request.LastName,
            request.Password);
    }

    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse(
            user.Id.Value,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CreatedAtUtc);
    }
}
