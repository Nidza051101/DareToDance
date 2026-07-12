namespace DareToDance.Application.Common.Security;

public record CurrentUser(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles);
