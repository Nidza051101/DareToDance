using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Infrastructure.MessageQueue;
using NotificationService.Infrastructure.Options;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Services;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Isti obrazac kao DareToDance.Infrastructure.DependencyInjection u
        // D2D Backend-u — TimeProvider.System, ne DateTime.UtcNow direktno,
        // da handleri ostanu testabilni (fake TimeProvider u testovima).
        services.AddSingleton(TimeProvider.System);

        services
            .AddOptions<EmailSettings>()
            .Bind(configuration.GetSection(EmailSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IEmailSender, GmailEmailSender>();

        // Isti Channel deli i pisanje (IMessageQueue) i čitanje (Reader u
        // NotificationQueueConsumer) — mora biti singleton.
        services.AddSingleton<InMemoryMessageQueue>();
        services.AddSingleton<IMessageQueue>(sp => sp.GetRequiredService<InMemoryMessageQueue>());

        // EF Core InMemory provajder — odluka admina da baza za testiranje
        // ostane privremena (v. napomenu na dnu NotificationDbContext.cs).
        // Podaci ne prežive restart procesa; to je namerno, ne propust.
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseInMemoryDatabase("notifications"));

        return services;
    }
}
