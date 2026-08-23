using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace DareToDance.Api.Common.Extensions;

public sealed class RateLimitSettings
{
    public const string SectionName = "RateLimitSettings";

    public int OtpRequestPermitLimit { get; init; } = 5;
    public int OtpRequestWindowMinutes { get; init; } = 15;
    public int OtpVerifyPermitLimit { get; init; } = 10;
    public int OtpVerifyWindowMinutes { get; init; } = 15;
}

public static class RateLimitingExtensions
{
    public static IServiceCollection AddApiRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection(RateLimitSettings.SectionName).Get<RateLimitSettings>()
                       ?? new RateLimitSettings();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // The sliding-window lease usually carries no RetryAfter metadata,
            // so a rejected caller would get a bare 429 — write the header
            // ourselves, falling back to a coarse hint.
            options.OnRejected = (context, _) =>
            {
                var retryAfterSeconds =
                    context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                        ? (int)retryAfter.TotalSeconds
                        : 60;

                context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString();

                return ValueTask.CompletedTask;
            };

            options.AddPolicy("otp-request", httpContext =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    ClientKey(httpContext),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = settings.OtpRequestPermitLimit,
                        Window = TimeSpan.FromMinutes(settings.OtpRequestWindowMinutes),
                        SegmentsPerWindow = 6,
                        QueueLimit = 0
                    }));

            options.AddPolicy("otp-verify", httpContext =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    ClientKey(httpContext),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = settings.OtpVerifyPermitLimit,
                        Window = TimeSpan.FromMinutes(settings.OtpVerifyWindowMinutes),
                        SegmentsPerWindow = 6,
                        QueueLimit = 0
                    }));
        });

        return services;
    }

    // These limits are transport-level defense in depth per client IP; the
    // per-account controls (cooldown, daily cap, attempt cap) live in the
    // handlers. RemoteIpAddress is null under TestServer and becomes the
    // proxy's address behind a reverse proxy — if a proxy ever fronts this
    // API, wire UseForwardedHeaders (with explicit KnownProxies) FIRST in
    // the pipeline or all clients collapse into one partition.
    private static string ClientKey(HttpContext httpContext)
        => httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
