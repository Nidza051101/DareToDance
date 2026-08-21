using DareToDance.Api.Features.Memberships.Shared;
using DareToDance.Api.IntgrationTests.Common;
using DareToDance.Domain.Membership;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace DareToDance.Api.IntgrationTests.Features.CreateMembershipTest;

[Collection("Integration Tests")]
public class CreateMembershipTest
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public CreateMembershipTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateMembership_Should_ReturnCreated_AndPersistActiveMembership_WhenValid()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("membership.valid@test.com", "Mila", "Milic");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        await AuthorizeAsync(scope, user);

        var validFrom = new DateTime(2026, 8, 21, 0, 0, 0, DateTimeKind.Utc);
        var validTo = validFrom.AddMonths(1);

        var request = new
        {
            UserId = user.Id.Value,
            ValidFrom = validFrom,
            ValidTo = validTo
        };

        // Act
        var response = await _client.PostAsJsonAsync("/memberships", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<MembershipResponse>();

        Assert.NotNull(body);
        Assert.Equal(user.Id.Value, body.UserId);
        Assert.Equal("Active", body.Status);

        var savedMembership = await dbContext.Memberships
            .AsNoTracking()
            .SingleAsync(m => m.UserId == user.Id);

        Assert.Equal(MembershipStatus.Active, savedMembership.Status);
        Assert.Equal(validFrom, savedMembership.ValidFrom);
        Assert.Equal(validTo, savedMembership.ValidTo);
    }

    [Fact]
    public async Task CreateMembership_Should_ReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var caller = User.Create("membership.caller@test.com", "Petar", "Petric");
        dbContext.Users.Add(caller);
        await dbContext.SaveChangesAsync();

        await AuthorizeAsync(scope, caller);

        var request = new
        {
            UserId = Guid.NewGuid(), // ne postoji u bazi
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddMonths(1)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/memberships", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateMembership_Should_ReturnUnauthorized_WhenNoTokenProvided()
    {
        // Arrange - namerno bez Authorization header-a
        _client.DefaultRequestHeaders.Authorization = null;

        var request = new
        {
            UserId = Guid.NewGuid(),
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddMonths(1)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/memberships", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task AuthorizeAsync(IServiceScope scope, User user)
    {
        var jwtTokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        var (accessToken, _) = jwtTokenGenerator.GenerateToken(user);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }
}
