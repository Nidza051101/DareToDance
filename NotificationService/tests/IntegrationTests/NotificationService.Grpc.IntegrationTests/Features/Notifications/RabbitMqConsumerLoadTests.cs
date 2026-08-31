using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NotificationService.Domain.NotificationRecord;
using NotificationService.Grpc.IntegrationTests.Common;
using NotificationService.Infrastructure.MessageQueue;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Grpc.IntegrationTests.Features.Notifications;

[Collection("rabbitmq")]
public sealed class RabbitMqConsumerLoadTests(RabbitMqFixture rabbit)
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(120);

    // TEST 1 — "load balancer": 1000 poruka, DVA radnika na istom redu.
    // RabbitMQ deli poruke između njih (competing consumers). Očekujemo:
    //  - svih 1000 poslato (ništa izgubljeno)
    //  - svaki primalac tačno jednom (ništa duplirano)
    //  - oba radnika su nešto uradila (posao je podeljen, ne radi jedan sve)
    //  - red prazan na kraju
    [Fact]
    public async Task TwoWorkers_SplitTheLoad_AndEachMessageIsSentExactlyOnce()
    {
        const int total = 1000;
        var uri = new Uri(rabbit.ConnectionString);
        var (dbName, root, log) = NewRun();

        var ids = LoadTestSupport.SeedPending(dbName, root, total, DateTime.UtcNow);
        await LoadTestSupport.PublishAsync(uri, ids);

        using var workerA = MakeWorker(uri, dbName, root, log, "A", fail: null);
        using var workerB = MakeWorker(uri, dbName, root, log, "B", fail: null);
        workerA.Start();
        workerB.Start();

        await LoadTestSupport.WaitUntilAsync(() => Task.FromResult(log.Total >= total), Timeout);

        Assert.Equal(total, log.Total);
        Assert.Equal(total, log.DistinctRecipients);
        Assert.True(log.ForWorker("A") > total / 10, $"Radnik A obradio samo {log.ForWorker("A")} od {total}");
        Assert.True(log.ForWorker("B") > total / 10, $"Radnik B obradio samo {log.ForWorker("B")} od {total}");
        Assert.Equal(0u, await LoadTestSupport.QueueDepthAsync(uri, RabbitMqTopology.EmailQueue));
    }

    // TEST 2 — jedan radnik i backlog od 1000. Prefetch drži memoriju ograničenom
    // (max ~100 nepotvrđenih odjednom), radnik ih izmelje sve serijski. Sporije
    // nego dva, ali pouzdano isprazni red.
    [Fact]
    public async Task OneWorker_DrainsA1000MessageBacklog()
    {
        const int total = 1000;
        var uri = new Uri(rabbit.ConnectionString);
        var (dbName, root, log) = NewRun();

        var ids = LoadTestSupport.SeedPending(dbName, root, total, DateTime.UtcNow);
        await LoadTestSupport.PublishAsync(uri, ids);

        using var worker = MakeWorker(uri, dbName, root, log, "solo", fail: null);
        worker.Start();

        await LoadTestSupport.WaitUntilAsync(() => Task.FromResult(log.Total >= total), Timeout);

        Assert.Equal(total, log.Total);
        Assert.Equal(total, log.DistinctRecipients);
        Assert.Equal(0u, await LoadTestSupport.QueueDepthAsync(uri, RabbitMqTopology.EmailQueue));
    }

    // TEST 3 — izolacija grešaka pod opterećenjem: 100 poruka, svaka 10. pada.
    // Loše poruke -> nack(requeue:false) -> DLQ; dobre prolaze normalno. Jedna
    // loša poruka NE blokira red.
    [Fact]
    public async Task FailingMessages_LandInDlq_WithoutBlockingTheRest()
    {
        const int total = 100;
        const int failEvery = 10;
        const int expectedOk = total - total / failEvery;   // 90
        const int expectedDead = total / failEvery;         // 10

        var uri = new Uri(rabbit.ConnectionString);
        var (dbName, root, log) = NewRun();

        var ids = LoadTestSupport.SeedPending(dbName, root, total, DateTime.UtcNow);
        await LoadTestSupport.PublishAsync(uri, ids);

        static bool Fail(string recipient) => LoadTestSupport.IndexFromRecipient(recipient) % failEvery == 0;

        using var worker = MakeWorker(uri, dbName, root, log, "solo", Fail);
        worker.Start();

        await LoadTestSupport.WaitUntilAsync(
            async () => log.Total >= expectedOk
                && await LoadTestSupport.QueueDepthAsync(uri, RabbitMqTopology.EmailDeadLetterQueue) >= expectedDead,
            Timeout);

        Assert.Equal(expectedOk, log.Total);
        Assert.Equal((uint)expectedDead, await LoadTestSupport.QueueDepthAsync(uri, RabbitMqTopology.EmailDeadLetterQueue));
        Assert.Equal(0u, await LoadTestSupport.QueueDepthAsync(uri, RabbitMqTopology.EmailQueue));

        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(dbName, root)
            .Options;
        await using var db = new NotificationDbContext(options);
        Assert.Equal(expectedOk, db.NotificationRecords.Count(r => r.Status == NotificationStatus.Sent));
        Assert.Equal(expectedDead, db.NotificationRecords.Count(r => r.Status == NotificationStatus.Failed));
    }

    private static (string DbName, InMemoryDatabaseRoot Root, SentLog Log) NewRun() =>
        ($"load-{Guid.NewGuid()}", new InMemoryDatabaseRoot(), new SentLog());

    private static WorkerFactory MakeWorker(
        Uri uri, string dbName, InMemoryDatabaseRoot root, SentLog log, string workerId, Func<string, bool>? fail) =>
        new()
        {
            RabbitMqUri = uri,
            DbName = dbName,
            DbRoot = root,
            SentLog = log,
            WorkerId = workerId,
            Fail = fail,
        };
}
