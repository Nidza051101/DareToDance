using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DareToDance.Api.IntgrationTests.Common;
using DareToDance.Domain.User;
using Microsoft.EntityFrameworkCore;

namespace DareToDance.Api.IntgrationTests.Features.Auth;

public class VerifyOtpTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private sealed record AuthResponseDto(string AccessToken, string TokenType, DateTime ExpiresAtUtc, Guid UserId);

    [Fact]
    public async Task CorrectCode_ReturnsToken_ThatOpensAuthorizedEndpoint_AndConsumesChallenge()
    {
        var (user, code) = await SeedAndRequestAsync("verify.happy@test.local");

        var response = await _client.PostAsJsonAsync(
            "/auth/otp/verify", new { email = "verify.happy@test.local", code });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);
        Assert.Equal("Bearer", auth.TokenType);
        Assert.Equal(user.Id.Value, auth.UserId);

        var meRequest = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var meResponse = await _client.SendAsync(meRequest);

        var wwwAuthenticate = string.Join(" | ", meResponse.Headers.WwwAuthenticate.Select(h => h.ToString()));
        Assert.True(
            meResponse.StatusCode == HttpStatusCode.OK,
            $"Expected 200, got {(int)meResponse.StatusCode}; WWW-Authenticate: {wwwAuthenticate}");
        var me = JsonDocument.Parse(await meResponse.Content.ReadAsStringAsync());
        Assert.Equal(user.Id.Value, me.RootElement.GetProperty("userId").GetGuid());

        var challenge = await factory.QueryAsync(db =>
            db.OtpChallenges.SingleAsync(c => c.UserId == user.Id));
        Assert.NotNull(challenge.ConsumedAtUtc);
    }

    [Fact]
    public async Task AuthorizedEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WrongCode_Returns401InvalidCode_AndBurnsOneAttempt()
    {
        var (user, code) = await SeedAndRequestAsync("verify.wrong@test.local");

        var response = await _client.PostAsJsonAsync(
            "/auth/otp/verify", new { email = "verify.wrong@test.local", code = WrongCode(code) });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Auth.InvalidCode", problem.RootElement.GetProperty("title").GetString());

        var challenge = await factory.QueryAsync(db =>
            db.OtpChallenges.SingleAsync(c => c.UserId == user.Id));
        Assert.Equal(1, challenge.FailedAttempts);
    }

    [Fact]
    public async Task AfterMaxFailedAttempts_EvenTheCorrectCode_Returns401()
    {
        var (_, code) = await SeedAndRequestAsync("verify.lockout@test.local");

        // MaxFailedAttempts is 3 (appsettings default).
        for (var i = 0; i < 3; i++)
        {
            await _client.PostAsJsonAsync(
                "/auth/otp/verify", new { email = "verify.lockout@test.local", code = WrongCode(code) });
        }

        var response = await _client.PostAsJsonAsync(
            "/auth/otp/verify", new { email = "verify.lockout@test.local", code });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ExpiredCode_Returns401()
    {
        var (_, code) = await SeedAndRequestAsync("verify.expired@test.local");

        factory.Time.Advance(TimeSpan.FromSeconds(61)); // ExpirySeconds is 60

        var response = await _client.PostAsJsonAsync(
            "/auth/otp/verify", new { email = "verify.expired@test.local", code });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ConsumedCode_CannotBeReplayed()
    {
        var (_, code) = await SeedAndRequestAsync("verify.replay@test.local");

        var first = await _client.PostAsJsonAsync(
            "/auth/otp/verify", new { email = "verify.replay@test.local", code });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var replay = await _client.PostAsJsonAsync(
            "/auth/otp/verify", new { email = "verify.replay@test.local", code });
        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
    }

    [Fact]
    public async Task UnknownEmail_AndWrongCode_AreIndistinguishable()
    {
        var (_, code) = await SeedAndRequestAsync("verify.oracle@test.local");

        var wrongCodeResponse = await _client.PostAsJsonAsync(
            "/auth/otp/verify", new { email = "verify.oracle@test.local", code = WrongCode(code) });
        var unknownEmailResponse = await _client.PostAsJsonAsync(
            "/auth/otp/verify", new { email = "verify.nobody@test.local", code = "123456" });

        Assert.Equal(HttpStatusCode.Unauthorized, wrongCodeResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmailResponse.StatusCode);

        // ProblemDetails carries a per-request traceId, so raw bodies can't be
        // compared — the enumeration contract is that every discriminating
        // field is identical.
        var wrongCodeProblem = JsonDocument.Parse(await wrongCodeResponse.Content.ReadAsStringAsync()).RootElement;
        var unknownEmailProblem = JsonDocument.Parse(await unknownEmailResponse.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(
            wrongCodeProblem.GetProperty("title").GetString(),
            unknownEmailProblem.GetProperty("title").GetString());
        Assert.Equal(
            wrongCodeProblem.GetProperty("detail").GetString(),
            unknownEmailProblem.GetProperty("detail").GetString());
        Assert.Equal(
            wrongCodeProblem.GetProperty("status").GetInt32(),
            unknownEmailProblem.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task MalformedCode_Gets400_AndBurnsNoAttempt()
    {
        var (user, _) = await SeedAndRequestAsync("verify.malformed@test.local");

        var tooShort = await _client.PostAsJsonAsync(
            "/auth/otp/verify", new { email = "verify.malformed@test.local", code = "123" });
        var nonDigits = await _client.PostAsJsonAsync(
            "/auth/otp/verify", new { email = "verify.malformed@test.local", code = "12ab56" });

        Assert.Equal(HttpStatusCode.BadRequest, tooShort.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, nonDigits.StatusCode);

        var challenge = await factory.QueryAsync(db =>
            db.OtpChallenges.SingleAsync(c => c.UserId == user.Id));
        Assert.Equal(0, challenge.FailedAttempts);
    }

    private async Task<(User User, string Code)> SeedAndRequestAsync(string email)
    {
        var user = await factory.SeedUserAsync(email);
        await _client.PostAsJsonAsync("/auth/otp/request", new { email });
        var notification = await factory.OtpSender.WaitForNotificationAsync();
        return (user, notification.Code);
    }

    private static string WrongCode(string code)
        => (code[0] == '9' ? "0" : "9") + code[1..];
}
