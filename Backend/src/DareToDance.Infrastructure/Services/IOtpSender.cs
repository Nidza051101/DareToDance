namespace DareToDance.Infrastructure.Services;

public interface IOtpSender
{
    Task SendAsync(OtpNotification notification, CancellationToken cancellationToken);
}

public sealed record OtpNotification(string Email, string Code, DateTime ExpiresAtUtc)
{
    public override string ToString()
        => $"OtpNotification {{ Email = {Email}, Code = [REDACTED], ExpiresAtUtc = {ExpiresAtUtc:O} }}";
}
