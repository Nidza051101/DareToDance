using DareToDance.Notifications.Grpc;
using Grpc.Core;
using NotificationService.Domain.NotificationRecord;
using NotificationService.Grpc.IntegrationTests.Common;
using NotificationServiceClient = DareToDance.Notifications.Grpc.NotificationService.NotificationServiceClient;

namespace NotificationService.Grpc.IntegrationTests.Features.Notifications;

public class SendNotificationTests(CustomWebApplicationFactory factory): IClassFixture<CustomWebApplicationFactory> {
    private readonly NotificationServiceClient _client = new(factory.CreateGrpcChannel());

    [Fact]
    public async Task ValidRequest_ReturnsAccepted_AndWritesRecordToDatabase()
    {
        // Arrange 
        var request = new SendNotificationRequest
        {
            Recipient = "test.user@example.com",
            Channel = Channel.Email,
            Template = "OtpCode",
            Variables =
            {
                ["code"] = "123456",
                ["expiresAtUtc"] = DateTime.UtcNow.ToString("O"),
            },
        };

        // Act
        var response = await _client.SendNotificationAsync(request);

        // Assert — deo 1: gRPC odgovor kaže "prihvaćeno" i daje TrackingId.
        Assert.True(response.Accepted);
        Assert.False(string.IsNullOrWhiteSpace(response.TrackingId));

        // Assert — deo 2: pravi dokaz da je nešto STVARNO upisano u bazu
        var recordId = Guid.Parse(response.TrackingId);
        var record = await factory.FindNotificationRecordAsync(recordId);

        Assert.NotNull(record);
        Assert.Equal(request.Recipient, record!.Recipient);
        Assert.Equal(NotificationStatus.Pending, record.Status);
    }

    [Fact]
    public async Task EmptyRecipient_ThrowsInvalidArgument_AndWritesNothing()
    {
        // Arrange 
        var request = new SendNotificationRequest
        {
            Recipient = "",
            Channel = Channel.Email,
            Template = "OtpCode",
        };

        // Act + Assert 
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _client.SendNotificationAsync(request).ResponseAsync);

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }
}
