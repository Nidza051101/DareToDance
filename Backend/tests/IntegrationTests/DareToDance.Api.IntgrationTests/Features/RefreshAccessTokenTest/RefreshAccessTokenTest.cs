using DareToDance.Api.Features.Auth.Shared;
using DareToDance.Api.IntgrationTests.Common;
using DareToDance.Domain.RefreshToken;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DareToDance.Api.IntgrationTests.Features.RefreshAccessTokenTest;

public class RefreshAccessTokenTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public RefreshAccessTokenTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RefreshAccessToken_Should_ReturnNewTokens_AndRotateOldOne_WhenValid()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var refreshTokenService = scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

        var user = User.Create("refresh.valid@test.com", "Ana", "Anic");
        dbContext.Users.Add(user);

        var utcNow = DateTime.UtcNow;
        var (rawToken, tokenHash, expiresAtUtc) = refreshTokenService.Generate(utcNow);
        var refreshToken = RefreshToken.Create(user.Id, tokenHash, utcNow, expiresAtUtc);
        dbContext.RefreshTokens.Add(refreshToken);

        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.PostAsJsonAsync("/auth/refresh", new { RefreshToken = rawToken });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthTokensResponse>();

        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(body.RefreshToken));
        Assert.NotEqual(rawToken, body.RefreshToken); // rotacija - mora biti nov token, ne isti

        var oldToken = await dbContext.RefreshTokens
            .AsNoTracking()
            .SingleAsync(rt => rt.Id == refreshToken.Id);

        Assert.NotNull(oldToken.RevokedAtUtc);
        Assert.NotNull(oldToken.ReplacedByTokenId);

        var newToken = await dbContext.RefreshTokens
            .AsNoTracking()
            .SingleAsync(rt => rt.Id == oldToken.ReplacedByTokenId);

        Assert.Null(newToken.RevokedAtUtc);
    }

    [Fact]
    public async Task RefreshAccessToken_Should_RevokeAllActiveTokens_WhenAlreadyRotatedTokenIsReused()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var refreshTokenService = scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

        var user = User.Create("refresh.stolen@test.com", "Marko", "Markovic");
        dbContext.Users.Add(user);

        var utcNow = DateTime.UtcNow;
        var (rawToken, tokenHash, expiresAtUtc) = refreshTokenService.Generate(utcNow);
        var refreshToken = RefreshToken.Create(user.Id, tokenHash, utcNow, expiresAtUtc);
        dbContext.RefreshTokens.Add(refreshToken);

        await dbContext.SaveChangesAsync();

        // Act - prvi refresh je legitiman, rotira token
        var firstResponse = await _client.PostAsJsonAsync("/auth/refresh", new { RefreshToken = rawToken });
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Act - drugi refresh sa ISTIM (vec rotiranim) tokenom simulira kradju
        var secondResponse = await _client.PostAsJsonAsync("/auth/refresh", new { RefreshToken = rawToken });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, secondResponse.StatusCode);

        var allTokensForUser = await dbContext.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.UserId == user.Id)
            .ToListAsync();

        Assert.Equal(2, allTokensForUser.Count); // originalni + jedan nastao rotacijom
        Assert.All(allTokensForUser, t => Assert.NotNull(t.RevokedAtUtc));
    }

    [Fact]
    public async Task RefreshAccessToken_Should_ReturnUnauthorized_WhenTokenIsExpired()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var refreshTokenService = scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

        var user = User.Create("refresh.expired@test.com", "Jovan", "Jovanovic");
        dbContext.Users.Add(user);

        var utcNow = DateTime.UtcNow;
        var (rawToken, tokenHash, _) = refreshTokenService.Generate(utcNow);
        var expiredToken = RefreshToken.Create(user.Id, tokenHash, utcNow.AddDays(-31), utcNow.AddDays(-1));
        dbContext.RefreshTokens.Add(expiredToken);

        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.PostAsJsonAsync("/auth/refresh", new { RefreshToken = rawToken });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshAccessToken_Should_ReturnUnauthorized_WhenTokenDoesNotExist()
    {
        // Act - nasumican string koji nikad nije izdat
        var response = await _client.PostAsJsonAsync("/auth/refresh", new { RefreshToken = Guid.NewGuid().ToString() });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshAccessToken_Should_ReturnUnauthorized_WhenUserIsBlocked()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var refreshTokenService = scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

        var user = User.Create("refresh.blocked@test.com", "Iva", "Ivic");
        dbContext.Users.Add(user);

        var utcNow = DateTime.UtcNow;
        var (rawToken, tokenHash, expiresAtUtc) = refreshTokenService.Generate(utcNow);
        var refreshToken = RefreshToken.Create(user.Id, tokenHash, utcNow, expiresAtUtc);
        dbContext.RefreshTokens.Add(refreshToken);

        user.Block(utcNow);

        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.PostAsJsonAsync("/auth/refresh", new { RefreshToken = rawToken });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
