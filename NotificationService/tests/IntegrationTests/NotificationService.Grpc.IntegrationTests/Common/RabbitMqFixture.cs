using Testcontainers.RabbitMq;

namespace NotificationService.Grpc.IntegrationTests.Common;

// Diže jedan RabbitMQ kontejner za ceo test klas (deli se između testova).
public sealed class RabbitMqFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _container =
        new RabbitMqBuilder("rabbitmq:3-management").Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

[CollectionDefinition("rabbitmq")]
public sealed class RabbitMqCollection : ICollectionFixture<RabbitMqFixture>;
