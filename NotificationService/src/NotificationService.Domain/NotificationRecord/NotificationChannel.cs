namespace NotificationService.Domain.NotificationRecord;

// Mora da prati Channel enum iz proto/notification.proto vrednost-po-vrednost —
// mapiranje se radi ručno u SendNotificationGrpcService, nema automatske veze.
public enum NotificationChannel
{
    Email = 0,
    Sms = 1,
    Push = 2,
}
