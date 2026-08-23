using DareToDance.Infrastructure.Options;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Persistence.Interceptors;
using DareToDance.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DareToDance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment)
    {
        services.AddSingleton<UpdateTimestampInterceptor>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            options
                .UseNpgsql(configuration.GetConnectionString("Database"))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(serviceProvider.GetRequiredService<UpdateTimestampInterceptor>()));

        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<OtpSettings>()
            .Bind(configuration.GetSection(OtpSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);

        services.AddSingleton<IOtpGenerator, OtpGenerator>();
        services.AddSingleton<IOtpCodeHasher, HmacOtpCodeHasher>();
        services.AddSingleton<ITokenService, TokenService>();

        if (isDevelopment)
        {
            services.AddSingleton<IOtpSender, ConsoleOtpSender>();
        }
        else
        {
            // Deliberate deployment block: ConsoleOtpSender prints plaintext codes
            // and must never run outside Development. Boot fails here until a real
            // sender (email/SMS) is implemented and registered for this branch.
            throw new InvalidOperationException(
                "No production OTP sender is configured. Implement a real IOtpSender " +
                "and register it for non-Development environments.");
        }

        return services;
    }
}
