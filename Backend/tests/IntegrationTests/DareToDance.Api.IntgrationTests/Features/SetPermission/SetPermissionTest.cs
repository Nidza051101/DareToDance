using DareToDance.Api.IntgrationTests.Common;
using DareToDance.Domain.PermissionEntity;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace DareToDance.Api.IntgrationTests.Features.SetUserPermissionTest;

public class SetUserPermissionTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public SetUserPermissionTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SetUserPermission_Should_ReturnOk_WhenAdminGrantsPermission()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("test@test.com", "Test", "User");
        var permission = Permission.Create("CanViewDashboard", "Allows user to view the dashboard");

        dbContext.Users.Add(user);
        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();

        await AuthorizeAsAdminAsync(scope);

        var request = new
        {
            UserId = user.Id.Value,
            PermissionId = permission.Id.Value,
            IsGranted = true
        };

        var response = await _client.PostAsJsonAsync("/permissions/set-permission", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

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