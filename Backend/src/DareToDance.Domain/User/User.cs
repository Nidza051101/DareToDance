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

    // OTP login code - one active code per user (no separate table/aggregate, kept simple on purpose)
    public string? LoginCodeHash { get; private set; }
    public DateTime? LoginCodeExpiresAtUtc { get; private set; }
    public DateTime? LoginCodeCreatedAtUtc { get; private set; }
    public int LoginCodeFailedAttempts { get; private set; }

    private User(
        UserId id,
        string firstName,
        string lastName,
        string email,
        string? phone,
        UserStatus status,
        UserRole userRole,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
        : base(id, createdAtUtc, updatedAtUtc)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Status = status;
        UserRole = userRole;
    }

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
            NormalizePhone(phone),
            UserStatus.Active,
            UserRole.Member,
            utcNow,
            utcNow);
    }

    // Strips everything except digits and a leading '+', so formatting differences
    // ("+381 60 111 2233" vs "+381601112233") don't cause lookup mismatches.
    public static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var trimmed = phone.Trim();
        var digits = new string(trimmed.Where(char.IsDigit).ToArray());

        return trimmed.StartsWith('+') ? $"+{digits}" : digits;
    }

    public bool HasActiveLoginCode(DateTime utcNow)
        => LoginCodeExpiresAtUtc is not null && utcNow < LoginCodeExpiresAtUtc;

    public void SetLoginCode(string codeHash, DateTime expiresAtUtc, DateTime utcNow)
    {
        LoginCodeHash = codeHash;
        LoginCodeExpiresAtUtc = expiresAtUtc;
        LoginCodeCreatedAtUtc = utcNow;
        LoginCodeFailedAttempts = 0;
        MarkAsUpdated(utcNow);
    }

    public void ClearLoginCode(DateTime utcNow)
    {
        LoginCodeHash = null;
        LoginCodeExpiresAtUtc = null;
        LoginCodeCreatedAtUtc = null;
        LoginCodeFailedAttempts = 0;
        MarkAsUpdated(utcNow);
    }

    public void RegisterLoginCodeFailedAttempt(DateTime utcNow)
    {
        LoginCodeFailedAttempts++;
        MarkAsUpdated(utcNow);
    }
}
