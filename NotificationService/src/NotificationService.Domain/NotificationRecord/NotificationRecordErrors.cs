using ErrorOr;

namespace NotificationService.Domain.NotificationRecord;

// Sve Unexpected namerno: handler validira ulaz pre poziva Create, pa okinut
// guard znači bug u pozivaocu, ne poslovni ishod — isti obrazac kao
// DareToDance.Domain.OtpChallenge.OtpChallengeErrors u D2D Backend-u.
public static class NotificationRecordErrors
{
    public static readonly Error RecipientRequired = Error.Unexpected(
        code: "NotificationRecord.RecipientRequired",
        description: "The recipient is required.");

    public static readonly Error TemplateRequired = Error.Unexpected(
        code: "NotificationRecord.TemplateRequired",
        description: "The template is required.");

    public static readonly Error AlreadyFinalized = Error.Unexpected(
        code: "NotificationRecord.AlreadyFinalized",
        description: "The notification has already been marked as sent or failed.");
}
