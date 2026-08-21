using DareToDance.Api.IntgrationTests.Common;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace DareToDance.Api.IntgrationTests.Features.UnblockUserTest;

[Collection("Integration Tests")]
public class UnblockUserTest
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public UnblockUserTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UnblockUser_Should_ReturnOk_AndSetStatusToActive_WhenUserIsBlocked()
    {
        // Arrange - korisnik se odmah pravi kao blokiran (isti Block() koji koristi i admin rucno).
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var user = User.Create(
            "jovana@test.com",
            "Jovana",
            "Jovanovic");

        user.Block(DateTime.UtcNow);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var jwtTokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

        var admin = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(u => u.UserRole == UserRole.Admin);

        var (accessToken, _) = jwtTokenGenerator.GenerateToken(admin);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var response = await _client.PostAsync($"/users/{user.Id.Value}/unblock", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedUser = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(u => u.Id == user.Id);

        Assert.Equal(UserStatus.Active, updatedUser.Status);
    }
}
