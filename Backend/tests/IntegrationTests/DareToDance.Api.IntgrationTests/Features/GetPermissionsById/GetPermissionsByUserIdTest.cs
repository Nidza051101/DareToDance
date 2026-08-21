using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DareToDance.Api.IntgrationTests.Common;
using DareToDance.Domain.PermissionEntity;
using DareToDance.Domain.User;
using DareToDance.Domain.UserPermission;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DareToDance.Api.IntgrationTests.Features.GetPermissionsByUserIdTest;

public sealed class GetPermissionsByUserIdTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public GetPermissionsByUserIdTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPermissionsByUserId_Should_ReturnAssignedPermissions_WhenUserHasPermissions()
    {
        var user = User.Create($"test_{Guid.NewGuid()}@test.com", "Test", "User");
        var permission = Permission.Create("Dance.Read", "Allows reading dance information.");
        var userPermission = UserPermission.Create(user.Id, permission.Id);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        dbContext.Users.Add(user);
        dbContext.Permissions.Add(permission);
        dbContext.UserPermissions.Add(userPermission);

        await dbContext.SaveChangesAsync();

        await AuthorizeAsAdminAsync(scope);

        var response = await _client.GetAsync($"/users/{user.Id.Value}/permissions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Čitanje kao JsonElement da se izbegne greška sa nedostatkom parametarskog/podrazumevanog konstruktora na domenskom entitetu
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(JsonValueKind.Array, result.ValueKind);
        Assert.Single(result.EnumerateArray().ToList());

        var firstPermission = result[0];
        Assert.Equal(permission.Name, firstPermission.GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetPermissionsByUserId_Should_ReturnEmptyList_WhenUserHasNoPermissions()
    {
        var user = User.Create($"test_{Guid.NewGuid()}@test.com", "Test", "User");

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        await AuthorizeAsAdminAsync(scope);

        var response = await _client.GetAsync($"/users/{user.Id.Value}/permissions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(JsonValueKind.Array, result.ValueKind);
        Assert.Empty(result.EnumerateArray());
    }

    [Fact]
    public async Task GetPermissionsByUserId_Should_ReturnUnauthorized_WhenNoTokenProvided()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync($"/users/{Guid.NewGuid()}/permissions");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPermissionsByUserId_Should_ReturnNotFound_WhenUserDoesNotExist()
    {
        using var scope = _factory.Services.CreateScope();

        await AuthorizeAsAdminAsync(scope);

        var response = await _client.GetAsync($"/users/{Guid.NewGuid()}/permissions");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task AuthorizeAsAdminAsync(IServiceScope scope)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwtTokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

        var admin = await dbContext.Users.AsNoTracking().SingleAsync(u => u.UserRole == UserRole.Admin);

        var (accessToken, _) = jwtTokenGenerator.GenerateToken(admin);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}

