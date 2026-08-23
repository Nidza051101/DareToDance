using DareToDance.Domain.Common;
using DareToDance.Domain.User.Id;

namespace DareToDance.Domain.User;

public sealed class User : AggregateRoot<UserId>
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }

    private User(
        UserId id,
        string firstName,
        string lastName,
        string email,
        string phone,
        DateTime createdAtUtc,
        DateTime updatedAtUtc
    )
        : base(id, createdAtUtc, updatedAtUtc)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
    }

    public static User Create(
        string firstName,
        string lastName,
        string email,
        string phone)
    {
        var utcNow = DateTime.UtcNow;

        return new User(
            UserId.CreateUnique(),
            firstName.Trim(),
            lastName.Trim(),
            email.Trim().ToLowerInvariant(),
            phone.Trim(),
            utcNow,
            utcNow);
    }
}