namespace DareToDance.Domain.Entities;

public class OtpCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string CodeHash { get; set; } = null!;
    public OtpPurpose Purpose { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int FailedAttempts { get; set; }
    public bool IsConsumed { get; set; }
}
