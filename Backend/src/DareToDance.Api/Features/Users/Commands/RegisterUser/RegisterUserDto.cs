namespace DareToDance.Api.Features.Users.Commands.RegisterUser;

public sealed class RegisterUserDto
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public string? Phone { get; init; }
}
