using DareToDance.Domain.Common;
using DareToDance.Domain.User.Id;

namespace DareToDance.Domain.User;

public sealed class User : AggregateRoot<UserId>
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string? Phone { get; private set; }
    public UserStatus Status { get; private set; }
    public UserRole UserRole { get; private set; }

    private User(
        UserId id,
        string firstName,
        string lastName,
        string email,
        string? phone,
        UserStatus status,
        UserRole role,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
        : base(id, createdAtUtc, updatedAtUtc)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Status = status;
        UserRole = role;
    }

    private User() { }

    public static User Create(
        string email,
        string firstName,
        string lastName,
        string? phone = null)
    {
        var utcNow = DateTime.UtcNow;

        return new User(
            UserId.CreateUnique(),
            firstName.Trim(),
            lastName.Trim(),
            email.Trim().ToLowerInvariant(),
            phone?.Trim(),
            UserStatus.Active,
            UserRole.Member,
            utcNow,
            utcNow);
    }
}
