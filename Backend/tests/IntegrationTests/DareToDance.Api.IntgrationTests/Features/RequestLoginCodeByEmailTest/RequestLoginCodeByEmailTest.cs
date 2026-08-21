using DareToDance.Api.IntgrationTests.Common;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DareToDance.Api.IntgrationTests.Features.RequestLoginCodeByEmailTest;

[Collection("Integration Tests")]
public class RequestLoginCodeByEmailTest
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public RequestLoginCodeByEmailTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RequestLoginCodeByEmail_Should_ReturnOk_WhenUserExists()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var user = User.Create(
            "ana@test.com",
            "Ana",
            "Anic");

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act
        var request = new
        {
            Email = "ana@test.com"
        };

        var response = await _client.PostAsJsonAsync("/auth/login/email", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedUser = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(u => u.Id == user.Id);

        Assert.NotNull(updatedUser.LoginCodeHash);
    }

    [Fact]
    public async Task RequestLoginCodeByEmail_Should_ReturnOk_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new
        {
            Email = "ne-postoji@test.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login/email", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RequestLoginCodeByEmail_Should_ReturnConflict_WhenCalledTwiceInARow()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var user = User.Create(
            "milica@test.com",
            "Milica",
            "Milic");

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var request = new
        {
            Email = "milica@test.com"
        };

        // Act
        await _client.PostAsJsonAsync("/auth/login/email", request);
        var secondResponse = await _client.PostAsJsonAsync("/auth/login/email", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }
}
