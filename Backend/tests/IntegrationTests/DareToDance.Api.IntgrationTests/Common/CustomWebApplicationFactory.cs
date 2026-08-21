using DareToDance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.UseSetting(
            "JwtSettings:Secret",
            "test-secret-key-that-is-long-enough-for-jwt");

        builder.UseSetting(
            "JwtSettings:ExpiryMinutes",
            "60");

        builder.UseSetting(
            "JwtSettings:Issuer",
            "DareToDance.Tests");

        builder.UseSetting(
            "JwtSettings:Audience",
            "DareToDance.Tests");

        builder.UseSetting(
            "ConnectionStrings:Database",
            _dbContainer.GetConnectionString());

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options
                    .UseNpgsql(_dbContainer.GetConnectionString())
                    .UseSnakeCaseNamingConvention());
        });
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
