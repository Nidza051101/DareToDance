using System.Net;
using System.Net.Http.Json;
using DareToDance.Api.IntgrationTests.Common;

namespace DareToDance.Api.IntgrationTests.Features.Auth;

public class GoogleLoginTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ExistingUser_ValidToken_Returns200()
    {
        
        var user = await factory.SeedUserAsync("google.login@test.local");

        
        factory.GoogleTokenValidator.EmailToReturn = user.Email;

        
        var response = await _client.PostAsJsonAsync(
            "/auth/google-login", new { idToken = "fake-token" });

        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UnknownUser_Returns404()
    {
        
        factory.GoogleTokenValidator.EmailToReturn = "nonexistent@test.local";

        
        var response = await _client.PostAsJsonAsync(
            "/auth/google-login", new { idToken = "fake-token" });

        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task InvalidToken_Returns400()
    {
        
        var response = await _client.PostAsJsonAsync(
            "/auth/google-login", new { idToken = "" });

        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

}