using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using NotificationService.Infrastructure.Persistence;
using NotificationRecordEntity = NotificationService.Domain.NotificationRecord.NotificationRecord;

namespace NotificationService.Grpc.IntegrationTests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"notifications-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("EmailSettings:GmailAddress", "test@example.com");
        builder.UseSetting("EmailSettings:AppPassword", "test-app-password");

        builder.UseSetting("RabbitMq:Host", "localhost");
        builder.UseSetting("RabbitMq:Username", "guest");
        builder.UseSetting("RabbitMq:Password", "guest");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<NotificationDbContext>>();
            services.AddDbContext<NotificationDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.RemoveAll<IHostedService>();
        });
    }
    public GrpcChannel CreateGrpcChannel()
    {
        var httpClient = CreateDefaultClient(new ResponseVersionHandler());

        return GrpcChannel.ForAddress(httpClient.BaseAddress!, new GrpcChannelOptions
        {
            HttpClient = httpClient,
        });
    }
    public async Task<NotificationRecordEntity?> FindNotificationRecordAsync(Guid id)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        return await dbContext.NotificationRecords.FindAsync(id);
    }
    private sealed class ResponseVersionHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            response.Version = request.Version;
            return response;
        }
    }
}
