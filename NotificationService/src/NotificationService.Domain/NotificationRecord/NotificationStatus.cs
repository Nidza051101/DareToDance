namespace NotificationService.Domain.NotificationRecord;

public enum NotificationStatus
{
    Pending = 0,  // upisano, čeka u redu
    Sent = 1,     // kanal (npr. Gmail) je potvrdio prijem
    Failed = 2,   // kanal je vratio grešku — kandidat za RetryFailedNotifications
}
