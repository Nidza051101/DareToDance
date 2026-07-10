namespace DareToDance.Api.Authentication.Contracts;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password
);