using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DareToDance.Api.IntgrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace DareToDance.Api.IntgrationTests.Features.Auth;

[Collection("Integration Tests")]
public class CompleteGoogleRegistrationTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private sealed record AuthResponseDto(
        string AccessToken,
        string TokenType,
        DateTime ExpiresAtUtc,
        Guid UserId,
        string RefreshToken,
        DateTime RefreshTokenExpiresAtUtc);

    [Fact]
    public async Task ValidToken_NewEmail_CreatesUser_AndReturnsToken()
    {
        var idToken = FakeGoogleToken("newperson@test.local", "Nova", "Osoba");

        var response = await _client.PostAsJsonAsync(
            "/auth/google/complete-registration",
            new { idToken, phone = "+381691110001" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);
        Assert.Equal("Bearer", auth.TokenType);
        Assert.False(string.IsNullOrEmpty(auth.RefreshToken));

        var user = await factory.QueryAsync(db =>
            db.Users.SingleAsync(u => u.Email == "newperson@test.local"));

        Assert.Equal(auth.UserId, user.Id.Value);
        Assert.Equal("Nova", user.FirstName);
        Assert.Equal("Osoba", user.LastName);
        Assert.Equal("+381691110001", user.Phone);
    }

    [Fact]
    public async Task ExistingEmail_Returns409()
    {
        await factory.SeedUserAsync("already-exists@test.local");
        var idToken = FakeGoogleToken("already-exists@test.local", "Neko", "Drugi");

        var response = await _client.PostAsJsonAsync(
            "/auth/google/complete-registration",
            new { idToken, phone = "+381691110002" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("User.DuplicateEmail", problem.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task ExistingPhone_Returns409()
    {
        var existing = await factory.SeedUserAsync("phone-owner@test.local");
        var idToken = FakeGoogleToken("someone-new@test.local", "Neko", "Nov");

        var response = await _client.PostAsJsonAsync(
            "/auth/google/complete-registration",
            new { idToken, phone = existing.Phone });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("User.DuplicatePhone", problem.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task MalformedToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync(
            "/auth/google/complete-registration",
            new { idToken = "not-a-real-token", phone = "+381691110003" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EmptyPhone_Returns400_AndNeverTouchesTheDatabase()
    {
        var idToken = FakeGoogleToken("nophone@test.local", "Bez", "Telefona");

        var response = await _client.PostAsJsonAsync(
            "/auth/google/complete-registration",
            new { idToken, phone = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var exists = await factory.QueryAsync(db =>
            db.Users.AnyAsync(u => u.Email == "nophone@test.local"));
        Assert.False(exists);
    }

    private static string FakeGoogleToken(string email, string firstName, string lastName)
    {
        var header = Base64UrlEncode("""{"alg":"RS256","typ":"JWT"}""");
        var payload = Base64UrlEncode(JsonSerializer.Serialize(new
        {
            email,
            given_name = firstName,
            family_name = lastName,
        }));

        return $"{header}.{payload}.fake-signature";
    }

    private static string Base64UrlEncode(string json)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
}
