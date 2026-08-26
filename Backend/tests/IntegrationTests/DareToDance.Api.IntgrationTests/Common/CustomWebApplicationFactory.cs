using DareToDance.Domain.User;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Testcontainers.PostgreSql;
using Xunit;

namespace DareToDance.Api.IntgrationTests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("daretodance_test")
        .WithUsername("daretodance")
        .WithPassword("daretodance")
        .Build();

    public CapturingOtpSender OtpSender { get; } = new();

    // Seeded from the real clock so JWTs minted with fake time stay valid
    // against JwtBearer's real-clock lifetime validation.
    public FakeTimeProvider Time { get; } = new(DateTimeOffset.UtcNow);

    public FakeGoogleTokenValidator GoogleTokenValidator { get; } = new();

    

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Development keeps ApplyMigrations() running inside the factory —
        // it is the only schema-creation mechanism for the test database.
        builder.UseEnvironment("Development");

        // Host settings win over appsettings and dev user-secrets.
        builder.UseSetting("ConnectionStrings:Database", _dbContainer.GetConnectionString());
        builder.UseSetting("JwtSettings:Secret", "test-secret-key-that-is-long-enough-for-jwt");
        builder.UseSetting("JwtSettings:ExpiryMinutes", "60");
        builder.UseSetting("JwtSettings:Issuer", "DareToDance.Tests");
        builder.UseSetting("JwtSettings:Audience", "DareToDance.Tests");
        builder.UseSetting("OtpSettings:Pepper", "test-pepper-0123456789abcdef0123456789abcdef");

        builder.UseSetting("GoogleAuth:ClientId", "test-google-client-id");

        // OTP timings pinned so the tests don't silently break when the
        // product values in appsettings are tuned. The test classes assume
        // exactly these numbers.
        builder.UseSetting("OtpSettings:ExpirySeconds", "60");
        builder.UseSetting("OtpSettings:ResendCooldownSeconds", "60");
        builder.UseSetting("OtpSettings:MaxFailedAttempts", "3");
        builder.UseSetting("OtpSettings:MaxCodesPerDay", "10");

        // Refresh lifetimes pinned for the same reason; RefreshTests and
        // LogoutTests advance FakeTimeProvider against exactly these numbers.
        builder.UseSetting("RefreshTokenSettings:SlidingLifetimeDays", "30");
        builder.UseSetting("RefreshTokenSettings:AbsoluteLifetimeDays", "90");

        // Relaxed transport limits so unrelated tests never 429 each other;
        // RateLimitTests re-tightens these in its own derived factory.
        builder.UseSetting("RateLimitSettings:OtpRequestPermitLimit", "10000");
        builder.UseSetting("RateLimitSettings:OtpVerifyPermitLimit", "10000");
        builder.UseSetting("RateLimitSettings:RefreshPermitLimit", "10000");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IOtpSender>();
            services.AddSingleton<IOtpSender>(OtpSender);

            // RemoveAll is required: TimeProvider.System is registered as an
            // instance, so a plain Add would append rather than replace.
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Time);
            services.RemoveAll<IGoogleTokenValidator>();
            services.AddSingleton<IGoogleTokenValidator>(GoogleTokenValidator);
        });
    }

    // No registration endpoint exists, so tests seed users directly.
    public async Task<User> SeedUserAsync(string email)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create(
            firstName: "Test",
            lastName: "User",
            email: email,
            phone: Guid.NewGuid().ToString("N")[..15]);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return user;
    }

    public async Task<T> QueryAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await query(dbContext);
    }

    public Task InitializeAsync()
    {
        return _dbContainer.StartAsync();
    }

    public new Task DisposeAsync()
    {
        return _dbContainer.StopAsync();
    }
}
