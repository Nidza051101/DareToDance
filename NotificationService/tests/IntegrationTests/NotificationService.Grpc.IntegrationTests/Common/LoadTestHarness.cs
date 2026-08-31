using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NotificationService.Domain.NotificationRecord;
using NotificationService.Grpc.Features.Notifications;
using NotificationService.Infrastructure.MessageQueue;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Services;
using RabbitMQ.Client;
using NotificationRecordEntity = NotificationService.Domain.NotificationRecord.NotificationRecord;

namespace NotificationService.Grpc.IntegrationTests.Common;

// Beleži svaki uspešan "send" — ukupno, po radniku, i koji primalac.
public sealed class SentLog
{
    private readonly ConcurrentBag<string> _recipients = [];
    private readonly ConcurrentDictionary<string, int> _perWorker = new();

    public void Record(string workerId, string recipient)
    {
        _recipients.Add(recipient);
        _perWorker.AddOrUpdate(workerId, 1, (_, n) => n + 1);
    }

    public int Total => _recipients.Count;
    public int DistinctRecipients => _recipients.Distinct().Count();
    public int ForWorker(string workerId) => _perWorker.GetValueOrDefault(workerId, 0);
}

// Lažni IEmailSender — ne ide na mrežu, samo beleži; opciono baci grešku za dati primalac.
public sealed class FakeEmailSender(SentLog log, string workerId, Func<string, bool>? fail) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        if (fail?.Invoke(message.Recipient) == true)
        {
            throw new InvalidOperationException("fake SMTP failure");
        }

        log.Record(workerId, message.Recipient);
        return Task.CompletedTask;
    }
}

// Jedan "radnik" = jedan host sa NotificationQueueConsumer-om, uperen na test broker
// i deljenu InMemory bazu. RetryFailedNotificationsJob je isključen.
public sealed class WorkerFactory : WebApplicationFactory<Program>
{
    public required Uri RabbitMqUri { get; init; }
    public required string DbName { get; init; }
    public required InMemoryDatabaseRoot DbRoot { get; init; }
    public required SentLog SentLog { get; init; }
    public required string WorkerId { get; init; }
    public Func<string, bool>? Fail { get; init; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("RabbitMq:Host", RabbitMqUri.Host);
        builder.UseSetting("RabbitMq:Port", RabbitMqUri.Port.ToString());
        builder.UseSetting("RabbitMq:Username", RabbitMqUri.UserInfo.Split(':')[0]);
        builder.UseSetting("RabbitMq:Password", RabbitMqUri.UserInfo.Split(':')[1]);
        builder.UseSetting("EmailSettings:GmailAddress", "test@example.com");
        builder.UseSetting("EmailSettings:AppPassword", "test");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<NotificationDbContext>>();
            services.AddDbContext<NotificationDbContext>(o => o.UseInMemoryDatabase(DbName, DbRoot));

            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(new FakeEmailSender(SentLog, WorkerId, Fail));

            services.RemoveAll<IHostedService>();
            services.AddHostedService<NotificationQueueConsumer>();
        });
    }

    // Pokreće host + hosted servise (consumer počne da čita).
    public void Start() => _ = Services;
}

public static class LoadTestSupport
{
    // Ubaci N Pending zapisa u deljenu bazu, vrati njihove Id-jeve (u redosledu).
    public static IReadOnlyList<Guid> SeedPending(
        string dbName, InMemoryDatabaseRoot root, int count, DateTime utcNow)
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(dbName, root)
            .Options;
        using var db = new NotificationDbContext(options);

        var ids = new List<Guid>(count);
        for (var i = 0; i < count; i++)
        {
            var vars = new Dictionary<string, string> { ["code"] = i.ToString("D6") };
            var record = NotificationRecordEntity
                .Create(RecipientFor(i), NotificationChannel.Email, "OtpCode", vars, utcNow)
                .Value;

            db.NotificationRecords.Add(record);
            ids.Add(record.Id);
        }

        db.SaveChanges();
        return ids;
    }

    // Objavi po jednu Email poruku za svaki Id — direktno na exchange (bez Osobe A).
    public static async Task PublishAsync(Uri rabbitUri, IReadOnlyList<Guid> recordIds)
    {
        var factory = new ConnectionFactory { Uri = rabbitUri };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();
        await RabbitMqTopology.DeclareAsync(channel);

        for (var i = 0; i < recordIds.Count; i++)
        {
            var msg = new QueuedNotification(
                recordIds[i], RecipientFor(i), NotificationChannel.Email, "OtpCode",
                new Dictionary<string, string> { ["code"] = i.ToString("D6") });

            await channel.BasicPublishAsync(
                RabbitMqTopology.Exchange,
                RabbitMqTopology.RoutingKeyFor(NotificationChannel.Email),
                JsonSerializer.SerializeToUtf8Bytes(msg));
        }
    }

    public static async Task<uint> QueueDepthAsync(Uri rabbitUri, string queue)
    {
        var factory = new ConnectionFactory { Uri = rabbitUri };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();
        var ok = await channel.QueueDeclarePassiveAsync(queue);
        return ok.MessageCount;
    }

    public static async Task WaitUntilAsync(Func<Task<bool>> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (await condition())
            {
                return;
            }

            await Task.Delay(200);
        }

        throw new TimeoutException("Uslov nije ispunjen u zadatom vremenu.");
    }

    public static string RecipientFor(int index) => $"user{index}@example.com";

    public static int IndexFromRecipient(string recipient) =>
        int.Parse(recipient["user".Length..recipient.IndexOf('@')]);
}
