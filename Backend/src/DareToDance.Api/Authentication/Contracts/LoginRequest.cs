namespace DareToDance.Api.Authentication.Contracts;

public record LoginRequest(
    string Email,
    string Password
);
