using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DareToDance.Api.IntgrationTests.Common;
using DareToDance.Domain.User;
using Microsoft.EntityFrameworkCore;

namespace DareToDance.Api.IntgrationTests.Features.Auth;

[Collection("Integration Tests")]
public class RefreshTests(CustomWebApplicationFactory factory)
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
    public async Task ValidToken_Rotates_AndTheNewPairWorks()
    {
        var (user, login) = await LoginAsync("refresh.happy@test.local");

        var response = await RefreshAsync(login.RefreshToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var refreshed = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(refreshed);
        Assert.Equal(user.Id.Value, refreshed.UserId);
        Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);

        var meRequest = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);
        var meResponse = await _client.SendAsync(meRequest);
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        // The rotation chain is recorded: the predecessor is consumed, points
        // at its live successor, and both belong to the same family.
        var tokens = await factory.QueryAsync(db =>
            db.RefreshTokens.Where(t => t.UserId == user.Id).ToListAsync());
        Assert.Equal(2, tokens.Count);

        var consumed = tokens.Single(t => t.ConsumedAtUtc != null);
        var live = tokens.Single(t => t.ConsumedAtUtc == null);
        Assert.Equal(live.Id, consumed.ReplacedById);
        Assert.Equal(consumed.FamilyId, live.FamilyId);
        Assert.Null(live.RevokedAtUtc);

        // And the chain keeps going.
        var second = await RefreshAsync(refreshed.RefreshToken);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
    }

    [Fact]
    public async Task ReusedToken_Returns401_AndRevokesTheWholeFamily()
    {
        var (user, login) = await LoginAsync("refresh.reuse@test.local");

        var firstRefresh = await RefreshAsync(login.RefreshToken);
        Assert.Equal(HttpStatusCode.OK, firstRefresh.StatusCode);
        var rotated = await firstRefresh.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(rotated);

        // Replaying the consumed predecessor is treated as theft...
        var replay = await RefreshAsync(login.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
        var problem = JsonDocument.Parse(await replay.Content.ReadAsStringAsync());
        Assert.Equal("Auth.InvalidToken", problem.RootElement.GetProperty("title").GetString());

        // ...so the live successor dies with the rest of the family.
        var successorAttempt = await RefreshAsync(rotated.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, successorAttempt.StatusCode);

        var tokens = await factory.QueryAsync(db =>
            db.RefreshTokens.Where(t => t.UserId == user.Id).ToListAsync());
        Assert.All(tokens, t => Assert.NotNull(t.RevokedAtUtc));
    }

    [Fact]
    public async Task WrongSecret_Returns401_ButDoesNotRevokeTheSession()
    {
        var (_, login) = await LoginAsync("refresh.wrongsecret@test.local");

        // Same token id, forged secret: knowing an id (from logs or traces)
        // must never be enough to kill a session.
        var idPart = login.RefreshToken.Split('.')[0];
        var forged = $"{idPart}.{new string('A', 43)}";

        var forgedResponse = await RefreshAsync(forged);
        Assert.Equal(HttpStatusCode.Unauthorized, forgedResponse.StatusCode);

        var genuine = await RefreshAsync(login.RefreshToken);
        Assert.Equal(HttpStatusCode.OK, genuine.StatusCode);
    }

    [Fact]
    public async Task ExpiredToken_Returns401()
    {
        var (_, login) = await LoginAsync("refresh.expired@test.local");

        factory.Time.Advance(TimeSpan.FromDays(31)); // SlidingLifetimeDays is 30

        var response = await RefreshAsync(login.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegularUse_SlidesTheSession_PastTheOriginalLifetime()
    {
        var (_, login) = await LoginAsync("refresh.sliding@test.local");

        factory.Time.Advance(TimeSpan.FromDays(20));
        var first = await RefreshAsync(login.RefreshToken);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var rotated = await first.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(rotated);

        // Day 40 — past the original 30-day lifetime, alive because of use.
        factory.Time.Advance(TimeSpan.FromDays(20));
        var second = await RefreshAsync(rotated.RefreshToken);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
    }

    [Fact]
    public async Task AbsoluteCap_ClampsExpiry_AndEndsTheSession()
    {
        // Read the clock BEFORE login: the whole login happens at this frozen
        // instant, so the family's absolute cap is exactly loginInstant + 90d.
        var loginInstant = factory.Time.GetUtcNow().UtcDateTime;
        var (_, login) = await LoginAsync("refresh.absolute@test.local");

        // Slide in 20-day steps to day 80 — each hop within the 30-day window.
        var wire = login.RefreshToken;
        AuthResponseDto? latest = null;

        for (var hop = 0; hop < 4; hop++)
        {
            factory.Time.Advance(TimeSpan.FromDays(20));
            var response = await RefreshAsync(wire);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            latest = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(latest);
            wire = latest.RefreshToken;
        }

        // At day 80 the sliding expiry (day 110) is clamped to the cap (day 90).
        // Millisecond tolerance: the cap round-trips through Postgres, whose
        // timestamptz truncates sub-microsecond ticks.
        Assert.Equal(loginInstant.AddDays(90), latest!.RefreshTokenExpiresAtUtc, TimeSpan.FromMilliseconds(1));

        // Day 95 — past the cap: no amount of use keeps the session alive.
        factory.Time.Advance(TimeSpan.FromDays(15));
        var afterCap = await RefreshAsync(wire);
        Assert.Equal(HttpStatusCode.Unauthorized, afterCap.StatusCode);
    }

    [Fact]
    public async Task FailureShapes_AreIndistinguishable()
    {
        var (_, login) = await LoginAsync("refresh.oracle@test.local");

        var malformed = await RefreshAsync("not-a-wire-token");
        var unknownId = await RefreshAsync($"{Guid.NewGuid():N}.{new string('B', 43)}");
        var wrongSecret = await RefreshAsync($"{login.RefreshToken.Split('.')[0]}.{new string('C', 43)}");

        // ProblemDetails carries a per-request traceId, so raw bodies can't be
        // compared — the contract is that every discriminating field matches.
        var problems = new List<JsonElement>();

        foreach (var response in new[] { malformed, unknownId, wrongSecret })
        {
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            problems.Add(JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement);
        }

        foreach (var problem in problems)
        {
            Assert.Equal(problems[0].GetProperty("title").GetString(), problem.GetProperty("title").GetString());
            Assert.Equal(problems[0].GetProperty("detail").GetString(), problem.GetProperty("detail").GetString());
            Assert.Equal(problems[0].GetProperty("status").GetInt32(), problem.GetProperty("status").GetInt32());
        }
    }

    [Fact]
    public async Task EmptyOrOversizedToken_Gets400BeforeTheHandler()
    {
        var empty = await RefreshAsync(string.Empty);
        var oversized = await RefreshAsync(new string('x', 300));

        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, oversized.StatusCode);
    }

    private async Task<(User User, AuthResponseDto Auth)> LoginAsync(string email)
    {
        var user = await factory.SeedUserAsync(email);

        var request = await _client.PostAsJsonAsync("/auth/otp/request", new { email });
        Assert.Equal(HttpStatusCode.Accepted, request.StatusCode);

        var notification = await factory.OtpSender.WaitForNotificationAsync();

        var verify = await _client.PostAsJsonAsync(
            "/auth/otp/verify", new { email, code = notification.Code });
        Assert.Equal(HttpStatusCode.OK, verify.StatusCode);

        var auth = await verify.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);

        return (user, auth);
    }

    private Task<HttpResponseMessage> RefreshAsync(string refreshToken)
        => _client.PostAsJsonAsync("/auth/refresh", new { refreshToken });
}
