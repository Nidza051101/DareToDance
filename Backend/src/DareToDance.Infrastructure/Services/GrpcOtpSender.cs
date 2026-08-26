using DareToDance.Notifications.Grpc;

namespace DareToDance.Infrastructure.Services;

// Produkciona implementacija IOtpSender-a — zamena za ConsoleOtpSender
// (DEV ONLY) van Development okruženja, v. DependencyInjection.cs. Poziva
// Notification servis preko gRPC-a i vraća se čim je zahtev PRIHVAĆEN u red,
// ne čeka da mejl stvarno bude poslat. V. artifact "Notification gRPC Flow".
internal sealed class GrpcOtpSender(
    NotificationService.NotificationServiceClient client) : IOtpSender
{
    public async Task SendAsync(OtpNotification notification, CancellationToken cancellationToken)
    {
        await client.SendNotificationAsync(
            new SendNotificationRequest
            {
                Recipient = notification.Email,
                Channel = Channel.Email,
                Template = "OtpCode",
                Variables =
                {
                    ["code"] = notification.Code,
                    ["expiresAtUtc"] = notification.ExpiresAtUtc.ToString("O"),
                },
            },
            cancellationToken: cancellationToken);
    }
}
