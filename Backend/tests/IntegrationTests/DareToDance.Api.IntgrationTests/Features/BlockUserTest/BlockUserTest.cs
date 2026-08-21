using DareToDance.Api.IntgrationTests.Common;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace DareToDance.Api.IntgrationTests.Features.BlockUserTest;

public class BlockUserTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public BlockUserTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task BlockUser_Should_ReturnOk_AndSetStatusToBlocked_WhenUserExists()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var user = User.Create(
            "petar@test.com",
            "Petar",
            "Petrovic");

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        await AuthorizeAsAdminAsync(scope);

        // Act
        var response = await _client.PostAsync($"/users/{user.Id.Value}/block", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedUser = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(u => u.Id == user.Id);

        Assert.Equal(UserStatus.Blocked, updatedUser.Status);
    }

    [Fact]
    public async Task BlockUser_Should_ReturnUnauthorized_WhenNoTokenProvided()
    {
        // Arrange - namerno bez Authorization header-a (cistimo ga za slucaj da je
        // ostao od nekog drugog testa u istoj klasi, koja deli isti _client).
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.PostAsync($"/users/{Guid.NewGuid()}/block", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task BlockUser_Should_ReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        await AuthorizeAsAdminAsync(scope);

        // Act
        var response = await _client.PostAsync($"/users/{Guid.NewGuid()}/block", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Seedovan admin nalog (iz UserConfiguration.HasData) - koristimo ga da dobijemo pravi JWT
    // sa Admin rolom, jer BlockUser/UnblockUser zahtevaju [Authorize(Roles = "Admin")].
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
