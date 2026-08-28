using Microsoft.Extensions.DependencyInjection;

namespace NotificationService.Infrastructure.MessageQueue;

// Dve "kuke" za registraciju RabbitMQ dela, namerno razdvojene:
//   AddNotificationPublisher -> Osoba A (slanje u red)
//   AddNotificationConsumer  -> Osoba B (čitanje iz reda)
// Svako menja SVOJU metodu, pa ne diraju iste linije -> merge bez konflikta.
// Obe se već pozivaju iz AddInfrastructure; za sada su prazne.
public static class MessageQueueRegistration
{
    // OSOBA A: ovde ide  services.AddSingleton<IMessageQueue, RabbitMqMessageQueue>();
    public static IServiceCollection AddNotificationPublisher(this IServiceCollection services)
    {
        return services;
    }

    // OSOBA B: ovde se preseli AddHostedService<NotificationQueueConsumer>() iz
    // Program.cs, uz prepisan consumer koji čita iz RabbitMQ-a.
    public static IServiceCollection AddNotificationConsumer(this IServiceCollection services)
    {
        return services;
    }
}
