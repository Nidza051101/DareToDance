using ErrorOr;

namespace NotificationService.Domain.NotificationRecord;

// Namerno bez Entity<TId>/AggregateRoot<TId>/ValueObject infrastrukture iz
// DareToDance.Domain.Common — ovaj servis je posebno rešenje (poseban .sln,
// poseban deploy), pa duplirati tu infrastrukturu ovde nije opravdano dok se
// stvarno ne pokaže potreba (domain eventi i sl.). Id je običan Guid za sada;
// ako kasnije zatreba jak tip (kao OtpChallengeId), lako se doda.
public sealed class NotificationRecord
{
    public Guid Id { get; }
    public string Recipient { get; }
    public NotificationChannel Channel { get; }
    public string Template { get; }

    // Sadržaj koji ide u šablon (npr. OTP kod, vreme isteka) — mora da se
    // sačuva ovde, ne samo prosledi u red, jer RetryFailedNotifications
    // ponovo šalje BAŠ ovaj zapis kasnije, kad poruka više nije u redu.
    // Bez ovoga bi retry poslao mejl sa praznim {{code}} mestom.
    public IReadOnlyDictionary<string, string> Variables { get; }

    public NotificationStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; }
    public DateTime? SentAtUtc { get; private set; }
    public string? FailureReason { get; private set; }
    public int RetryCount { get; private set; }

    private NotificationRecord(
        Guid id,
        string recipient,
        NotificationChannel channel,
        string template,
        IReadOnlyDictionary<string, string> variables,
        DateTime createdAtUtc)
    {
        Id = id;
        Recipient = recipient;
        Channel = channel;
        Template = template;
        Variables = variables;
        Status = NotificationStatus.Pending;
        CreatedAtUtc = createdAtUtc;
        RetryCount = 0;
    }

    public static ErrorOr<NotificationRecord> Create(
        string recipient,
        NotificationChannel channel,
        string template,
        IReadOnlyDictionary<string, string> variables,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(recipient))
        {
            return NotificationRecordErrors.RecipientRequired;
        }

        if (string.IsNullOrWhiteSpace(template))
        {
            return NotificationRecordErrors.TemplateRequired;
        }

        return new NotificationRecord(Guid.CreateVersion7(), recipient, channel, template, variables, utcNow);
    }

    // Poziva Worker/Handler pošto kanal (npr. Gmail) potvrdi isporuku.
    public ErrorOr<Success> MarkSent(DateTime utcNow)
    {
        if (Status != NotificationStatus.Pending)
        {
            return NotificationRecordErrors.AlreadyFinalized;
        }

        Status = NotificationStatus.Sent;
        SentAtUtc = utcNow;
        return Result.Success;
    }

    // failureReason ostaje u bazi radi dijagnostike; RetryFailedNotifications
    // koristi RetryCount da izbegne beskonačno ponavljanje istog zahteva.
    public ErrorOr<Success> MarkFailed(string failureReason)
    {
        if (Status == NotificationStatus.Sent)
        {
            return NotificationRecordErrors.AlreadyFinalized;
        }

        Status = NotificationStatus.Failed;
        FailureReason = failureReason;
        RetryCount++;
        return Result.Success;
    }

    // Vraća u Pending da Worker može ponovo da je obradi.
    public void ResetForRetry()
    {
        Status = NotificationStatus.Pending;
    }
}
