namespace NotificationService.Infrastructure.Options;

public sealed class RetryPolicySettings
{
    public const string SectionName = "RetryPolicySettings";

    public int IntervalMinutes { get; init; } = 1;

    public int MaxAttempts { get; init; } = 3;
}
