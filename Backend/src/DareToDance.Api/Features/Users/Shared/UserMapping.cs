using DareToDance.Domain.User;

namespace DareToDance.Api.Features.Users.Shared;

public static class UserMapping
{
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
