using DareToDance.Application.Common.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DareToDance.Api.IntegrationTests.TestUtils;

public class DareToDanceApiFactory : WebApplicationFactory<Program>
{
    public CapturingEmailSender EmailSender { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // added after the app's own sources, so these values win regardless of
        // local appsettings.Development.json / user secrets — tests stay hermetic
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = "integration-test-secret-that-is-long-enough-for-hmac-sha256",
                ["JwtSettings:ExpiryMinutes"] = "60",
                ["JwtSettings:Issuer"] = "DareToDance.IntegrationTests",
                ["JwtSettings:Audience"] = "DareToDance.IntegrationTests",
                ["OtpSettings:CodeLength"] = "6",
                ["OtpSettings:ExpiryMinutes"] = "5",
                ["OtpSettings:MaxFailedAttempts"] = "5",
                ["OtpSettings:ResendCooldownSeconds"] = "60",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(EmailSender);
        });
    }
}
