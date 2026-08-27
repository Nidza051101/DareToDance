namespace NotificationService.Grpc.Features.Notifications.Commands.RetryFailedNotifications;

// Skelet za "Failed Notification Scheduled Job" sa artifact-a — namerno
// nedovršen. Otvorene odluke pre implementacije (v. artifact, notes):
//   - interval čitanja (npr. svakih 5 min?)
//   - max broj pokušaja pre trajnog odustajanja (NotificationRecord.RetryCount
//     već postoji za ovo)
//   - backoff između pokušaja, da se Gmail ne "bombarduje" istim zahtevom
// Kad se ovo reši: pročitati NotificationRecords sa Status == Failed i
// RetryCount < max iz NotificationDbContext, pozvati record.ResetForRetry(),
// i ponovo ih ubaciti u IMessageQueue — isti put kao SendNotification.Handler.
public sealed class RetryFailedNotificationsJob : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // TODO: implementirati periodičnu proveru — v. napomenu iznad.
        return Task.CompletedTask;
    }
}
