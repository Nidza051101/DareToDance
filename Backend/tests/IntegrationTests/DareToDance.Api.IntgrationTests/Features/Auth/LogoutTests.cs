using System.Net;
using System.Net.Http.Json;
using DareToDance.Api.IntgrationTests.Common;

namespace DareToDance.Api.IntgrationTests.Features.Auth;

[Collection("Integration Tests")]
public class LogoutTests(CustomWebApplicationFactory factory)
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
    public async Task Logout_Returns204_AndTheSessionStopsRefreshing()
    {
        var auth = await SeedAndLoginAsync("logout.happy@test.local");

        var logout = await LogoutAsync(auth.RefreshToken);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var refresh = await RefreshAsync(auth.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task Logout_WithARotatedToken_StillKillsTheSession()
    {
        var auth = await SeedAndLoginAsync("logout.rotated@test.local");

        var refreshed = await (await RefreshAsync(auth.RefreshToken))
            .Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(refreshed);

        // The client signs out with the stale predecessor it still has on
        // disk: possession of any genuine token of the family is the logout
        // capability, so the live successor dies too.
        var logout = await LogoutAsync(auth.RefreshToken);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var successorRefresh = await RefreshAsync(refreshed.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, successorRefresh.StatusCode);
    }

    [Fact]
    public async Task Logout_OnlyKillsItsOwnSession()
    {
        await factory.SeedUserAsync("logout.devices@test.local");

        // Two logins = two devices = two independent families. The second
        // request must wait out the resend cooldown (60s).
        var phone = await LoginExistingAsync("logout.devices@test.local");
        factory.Time.Advance(TimeSpan.FromSeconds(61));
        var tablet = await LoginExistingAsync("logout.devices@test.local");

        var logout = await LogoutAsync(phone.RefreshToken);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var phoneRefresh = await RefreshAsync(phone.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, phoneRefresh.StatusCode);

        var tabletRefresh = await RefreshAsync(tablet.RefreshToken);
        Assert.Equal(HttpStatusCode.OK, tabletRefresh.StatusCode);
    }

    [Fact]
    public async Task Logout_WithGarbageOrUnknownTokens_IsSilently204()
    {
        var garbage = await LogoutAsync("not-a-wire-token");
        var unknown = await LogoutAsync($"{Guid.NewGuid():N}.{new string('D', 43)}");

        Assert.Equal(HttpStatusCode.NoContent, garbage.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, unknown.StatusCode);
    }

    private async Task<AuthResponseDto> SeedAndLoginAsync(string email)
    {
        await factory.SeedUserAsync(email);
        return await LoginExistingAsync(email);
    }

    private async Task<AuthResponseDto> LoginExistingAsync(string email)
    {
        var request = await _client.PostAsJsonAsync("/auth/otp/request", new { email });
        Assert.Equal(HttpStatusCode.Accepted, request.StatusCode);

        var notification = await factory.OtpSender.WaitForNotificationAsync();

        var verify = await _client.PostAsJsonAsync(
            "/auth/otp/verify", new { email, code = notification.Code });
        Assert.Equal(HttpStatusCode.OK, verify.StatusCode);

        var auth = await verify.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);
        return auth;
    }

    // Isti razlog kao u RefreshTests.cs — endpoint sad čita iz kolačića.
    private Task<HttpResponseMessage> RefreshAsync(string refreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");
        request.Headers.Add("Cookie", $"refreshToken={refreshToken}");
        return _client.SendAsync(request);
    }

    // Isti razlog kao RefreshAsync iznad — endpoint sad čita iz kolačića.
    private Task<HttpResponseMessage> LogoutAsync(string refreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/auth/logout");
        request.Headers.Add("Cookie", $"refreshToken={refreshToken}");
        return _client.SendAsync(request);
    }
}
