using NotificationService.Grpc.Common.Extensions;
using NotificationService.Grpc.Features.Notifications;
using NotificationService.Grpc.Features.Notifications.Commands.RetryFailedNotifications;
using NotificationService.Grpc.Features.Notifications.Commands.SendNotification;
using NotificationService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddGrpc();
    builder.Services.AddPresentation();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddHostedService<NotificationQueueConsumer>();
    builder.Services.AddHostedService<RetryFailedNotificationsJob>();
}

var app = builder.Build();
{
    app.MapGrpcService<SendNotificationGrpcService>();

    app.MapGet("/", () =>
        "NotificationService.Grpc — komunikacija ide preko gRPC klijenta, ne browsera.");

    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });

    app.Run();
}

public partial class Program { }
