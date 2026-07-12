namespace DareToDance.Application.Services.Authentication.Jwt;

public record TokenSubject(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    IReadOnlyList<string> Roles);
