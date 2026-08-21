using DareToDance.Api.IntgrationTests.Common;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using DareToDance.Domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace DareToDance.Api.IntgrationTests.Features.CreatePermissionTest;

public class CreatePermissionTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public CreatePermissionTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreatePermission_Should_ReturnCreated_WhenAdminCreatesPermission()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        await AuthorizeAsAdminAsync(scope);

        var request = new
        {
            Name = "CanViewDashboard",
            Description = "Allows user to view the dashboard"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/permissions/create", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private async Task AuthorizeAsAdminAsync(IServiceScope scope)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwtTokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

        var admin = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(u => u.UserRole == UserRole.Admin);

        var (accessToken, _) = jwtTokenGenerator.GenerateToken(admin);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }
}