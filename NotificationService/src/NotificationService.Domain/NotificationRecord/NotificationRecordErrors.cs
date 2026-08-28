using ErrorOr;

namespace NotificationService.Domain.NotificationRecord;

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
