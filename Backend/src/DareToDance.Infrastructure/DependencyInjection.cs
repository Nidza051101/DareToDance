using DareToDance.Infrastructure.Options;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Persistence.Interceptors;
using DareToDance.Infrastructure.Services;
using DareToDance.Notifications.Grpc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

        services.AddOptions<RefreshTokenSettings>()
            .Bind(configuration.GetSection(RefreshTokenSettings.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                s => s.AbsoluteLifetimeDays >= s.SlidingLifetimeDays,
                "RefreshTokenSettings:AbsoluteLifetimeDays must be greater than or equal to SlidingLifetimeDays.")
            .ValidateOnStart();

        services.AddOptions<GoogleAuthSettings>()
            .Bind(configuration.GetSection(GoogleAuthSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<NotificationServiceSettings>()
            .Bind(configuration.GetSection(NotificationServiceSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Typed gRPC klijent — adresa se čita tek pri prvom pozivu (lazy),
        // pa ValidateOnStart iznad hvata prazan/loš config pre toga.
        services.AddGrpcClient<NotificationService.NotificationServiceClient>(
            (serviceProvider, options) =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<NotificationServiceSettings>>().Value;
                options.Address = new Uri(settings.GrpcAddress);
            });

        services.AddSingleton(TimeProvider.System);

        services.AddSingleton<IOtpGenerator, OtpGenerator>();
        services.AddSingleton<IOtpCodeHasher, HmacOtpCodeHasher>();
        services.AddSingleton<IRefreshTokenHasher, Sha256RefreshTokenHasher>();
        services.AddSingleton<ITokenService, TokenService>();

        if (isDevelopment)
        {
            services.AddSingleton<IOtpSender, ConsoleOtpSender>();

            services.AddSingleton<IGoogleTokenVerifier, PlaceholderGoogleTokenVerifier>();
        }
        else
        {
            // ConsoleOtpSender prints plaintext codes and must never run outside
            // Development — GrpcOtpSender (v. Services/GrpcOtpSender.cs) je prava
            // implementacija, poziva Notification servis preko gRPC-a.
            services.AddScoped<IOtpSender, GrpcOtpSender>();

            // NAMERNO NEDIRANO — IGoogleTokenVerifier i dalje nema registraciju
            // van Development. To je postojeći, odvojen gap (PlaceholderGoogleTokenVerifier
            // nije prava implementacija, isto kao ConsoleOtpSender pre ove izmene) —
            // van obima ove sesije (Notification/gRPC), ne rešava se ovde.
        }

        return services;
    }
}
