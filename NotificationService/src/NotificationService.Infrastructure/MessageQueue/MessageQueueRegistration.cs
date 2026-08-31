using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NotificationService.Infrastructure.Options;
using RabbitMQ.Client;

namespace NotificationService.Infrastructure.MessageQueue;

// Dve "kuke" za registraciju RabbitMQ dela, namerno razdvojene:
//   AddNotificationPublisher -> Osoba A (slanje u red)
//   AddNotificationConsumer  -> Osoba B (čitanje iz reda)
// Svako menja SVOJU metodu, pa ne diraju iste linije -> merge bez konflikta.
// Obe se već pozivaju iz AddInfrastructure; za sada su prazne.
public static class MessageQueueRegistration
{
    public static IServiceCollection AddNotificationPublisher(this IServiceCollection services)
    {
        services.AddSingleton<IMessageQueue, RabbitMqMessageQueue>();

        services.AddHealthChecks()
                .AddRabbitMQ(name: "rabbitmq", tags: ["ready"]);

        return services;
    }

    // OSOBA B: ovde se preseli AddHostedService<NotificationQueueConsumer>() iz
    // Program.cs, uz prepisan consumer koji čita iz RabbitMQ-a.
    public static IServiceCollection AddNotificationConsumer(this IServiceCollection services)
    {
        return services;
    }
}
