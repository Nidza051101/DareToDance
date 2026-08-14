using DareToDance.Domain.Common;
using DareToDance.Domain.User.Id;

namespace DareToDance.Domain.User;

public sealed class User : Entity<UserId>
{
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string PasswordHash { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private User(
        UserId id,
        string email,
        string firstName,
        string lastName,
        string passwordHash,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
        : base(id)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = passwordHash;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public static User Create(string email, string firstName, string lastName, string passwordHash)
    {
        var utcNow = DateTime.UtcNow;

        return new User(
            UserId.CreateUnique(),
            email.Trim().ToLowerInvariant(),
            firstName.Trim(),
            lastName.Trim(),
            passwordHash,
            utcNow,
            utcNow);
    }
}