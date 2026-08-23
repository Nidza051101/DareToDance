using System.Net;
using System.Net.Http.Json;
using DareToDance.Api.IntgrationTests.Common;
using Microsoft.AspNetCore.Hosting;

namespace DareToDance.Api.IntgrationTests.Features.Auth;

// Own factory: tight limits here must not bleed into the other test classes.
public sealed class TightRateLimitFactory : CustomWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseSetting("RateLimitSettings:OtpRequestPermitLimit", "3");
        builder.UseSetting("RateLimitSettings:OtpRequestWindowMinutes", "15");
    }
}

public class RateLimitTests(TightRateLimitFactory factory) : IClassFixture<TightRateLimitFactory>
{
    [Fact]
    public async Task RequestsOverTheLimit_Get429WithRetryAfter()
    {
        // TestServer has no RemoteIpAddress, so every request shares the
        // "unknown" partition — which is exactly what this test needs.
        var client = factory.CreateClient();

        for (var i = 0; i < 3; i++)
        {
            var accepted = await client.PostAsJsonAsync(
                "/auth/otp/request", new { email = "rate.limit@test.local" });
            Assert.Equal(HttpStatusCode.Accepted, accepted.StatusCode);
        }

        var limited = await client.PostAsJsonAsync(
            "/auth/otp/request", new { email = "rate.limit@test.local" });

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.True(limited.Headers.Contains("Retry-After"));
    }
}
