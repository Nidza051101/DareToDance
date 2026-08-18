using DareToDance.Domain.Common;
using DareToDance.Domain.User.Id;

namespace DareToDance.Domain.User;

public sealed class User : Entity<UserId>
{
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public string Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private User(
        UserId id,
        string fullName,
        string email,
        string phone,
        string status,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
        : base(id)
    {
        FullName = fullName;
        Email = email;
        Phone = phone;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public static User Create(string fullName, string email, string phone)
    {
        var utcNow = DateTime.UtcNow;

        return new User(
            UserId.CreateUnique(),
            fullName.Trim(),
            email.Trim().ToLowerInvariant(),
            phone.Trim(),
            "active",
            utcNow,
            utcNow);
    }
}
