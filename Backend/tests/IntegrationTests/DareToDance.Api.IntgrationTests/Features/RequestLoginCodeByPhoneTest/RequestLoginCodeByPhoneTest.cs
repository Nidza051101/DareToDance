using DareToDance.Api.IntgrationTests.Common;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DareToDance.Api.IntgrationTests.Features.RequestLoginCodeByPhoneTest;

public class RequestLoginCodeByPhoneTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public RequestLoginCodeByPhoneTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RequestLoginCodeByPhone_Should_ReturnOk_WhenUserExists()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var user = User.Create(
            "marko@test.com",
            "Marko",
            "Markovic",
            "+381601112233");

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act
        var request = new
        {
            Phone = "+381 60 111 2233"
        };

        var response = await _client.PostAsJsonAsync("/auth/login/phone", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedUser = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(u => u.Id == user.Id);

        Assert.NotNull(updatedUser.LoginCodeHash);
    }
}
