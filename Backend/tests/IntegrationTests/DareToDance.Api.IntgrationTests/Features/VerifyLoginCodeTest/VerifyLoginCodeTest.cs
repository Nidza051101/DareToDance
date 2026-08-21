using DareToDance.Api.IntgrationTests.Common;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DareToDance.Api.IntgrationTests.Features;

[Collection("Integration Tests")]
public class VerifyLoginCodeTest
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public VerifyLoginCodeTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task VerifyLoginCode_Should_ReturnOk()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var user = User.Create(
            "verify.login.code@test.com",
            "John",
            "Doe");

        var passwordHasher = scope.ServiceProvider
            .GetRequiredService<IPasswordHasher>();

        const string code = "123456";

        var codeHash = passwordHasher.Hash(code);

        user.SetLoginCode(
            codeHash,
            DateTime.UtcNow.AddMinutes(5),
            DateTime.UtcNow);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var request = new
        {
            Recipient = "verify.login.code@test.com",
            Code = "123456"
        };

        var response = await _client.PostAsJsonAsync("/auth/login/verify",request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}