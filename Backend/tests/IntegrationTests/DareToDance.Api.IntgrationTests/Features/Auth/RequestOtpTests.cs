using System.Net;
using System.Net.Http.Json;
using DareToDance.Api.IntgrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace DareToDance.Api.IntgrationTests.Features.Auth;

public class RequestOtpTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ExistingUser_Gets202_CodeIsSent_AndOnlyHashIsStored()
    {
        var user = await factory.SeedUserAsync("request.happy@test.local");

        // Mixed casing proves the handler normalizes like User.Create does.
        var response = await _client.PostAsJsonAsync(
            "/auth/otp/request", new { email = "Request.Happy@Test.Local" });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var notification = await factory.OtpSender.WaitForNotificationAsync();
        Assert.Equal("request.happy@test.local", notification.Email);
        Assert.Matches("^[0-9]{6}$", notification.Code);

        var challenge = await factory.QueryAsync(db =>
            db.OtpChallenges.SingleAsync(c => c.UserId == user.Id));

        Assert.NotEqual(notification.Code, challenge.CodeHash);
        Assert.Null(challenge.ConsumedAtUtc);
        Assert.Null(challenge.InvalidatedAtUtc);
    }

    [Fact]
    public async Task UnknownEmail_GetsByteIdentical202_AndNothingIsSent()
    {
        await factory.SeedUserAsync("request.known@test.local");

        var knownResponse = await _client.PostAsJsonAsync(
            "/auth/otp/request", new { email = "request.known@test.local" });
        await factory.OtpSender.WaitForNotificationAsync();

        var unknownResponse = await _client.PostAsJsonAsync(
            "/auth/otp/request", new { email = "request.ghost@test.local" });

        Assert.Equal(HttpStatusCode.Accepted, unknownResponse.StatusCode);
        Assert.Equal(
            await knownResponse.Content.ReadAsStringAsync(),
            await unknownResponse.Content.ReadAsStringAsync());
        Assert.False(factory.OtpSender.TryTakeNotification(out _));
    }

    [Fact]
    public async Task SecondRequestWithinCooldown_Gets202_ButSendsNothing()
    {
        await factory.SeedUserAsync("request.cooldown@test.local");

        await _client.PostAsJsonAsync("/auth/otp/request", new { email = "request.cooldown@test.local" });
        await factory.OtpSender.WaitForNotificationAsync();

        var response = await _client.PostAsJsonAsync(
            "/auth/otp/request", new { email = "request.cooldown@test.local" });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.False(factory.OtpSender.TryTakeNotification(out _));
    }

    [Fact]
    public async Task RequestAfterExpiryAndCooldown_SendsFreshCode_AndInvalidatesOldChallenge()
    {
        // Regression guard: an expired-but-unconsumed row must not keep the
        // partial unique index slot occupied — the invalidation predicate has
        // to cover expired rows too, or this user never gets a code again.
        var user = await factory.SeedUserAsync("request.supersede@test.local");

        await _client.PostAsJsonAsync("/auth/otp/request", new { email = "request.supersede@test.local" });
        await factory.OtpSender.WaitForNotificationAsync();

        factory.Time.Advance(TimeSpan.FromSeconds(65)); // past both expiry (60s) and cooldown (60s)

        var response = await _client.PostAsJsonAsync(
            "/auth/otp/request", new { email = "request.supersede@test.local" });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var second = await factory.OtpSender.WaitForNotificationAsync();
        Assert.Matches("^[0-9]{6}$", second.Code);

        var challenges = await factory.QueryAsync(db =>
            db.OtpChallenges.Where(c => c.UserId == user.Id).ToListAsync());

        Assert.Equal(2, challenges.Count);
        Assert.Single(challenges, c => c.ConsumedAtUtc == null && c.InvalidatedAtUtc == null);
    }

    [Fact]
    public async Task DailyCap_SilentlyStopsSending()
    {
        await factory.SeedUserAsync("request.cap@test.local");

        // MaxCodesPerDay is 10 (appsettings default).
        for (var i = 0; i < 10; i++)
        {
            await _client.PostAsJsonAsync("/auth/otp/request", new { email = "request.cap@test.local" });
            await factory.OtpSender.WaitForNotificationAsync();
            factory.Time.Advance(TimeSpan.FromSeconds(61));
        }

        var response = await _client.PostAsJsonAsync(
            "/auth/otp/request", new { email = "request.cap@test.local" });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.False(factory.OtpSender.TryTakeNotification(out _));
    }

    [Fact]
    public async Task MalformedEmail_Gets400()
    {
        var response = await _client.PostAsJsonAsync("/auth/otp/request", new { email = "not-an-email" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
