using DareToDance.Infrastructure.Services;

namespace DareToDance.Api.IntgrationTests.Common;

public sealed class FakeGoogleTokenValidator : IGoogleTokenValidator
{
    public string EmailToReturn { get; set; } = "test@test.com";

    public Task<GoogleTokenPayload> ValidateAsync(string idToken, string clientId)
    {
        if (string.IsNullOrEmpty(idToken))
            throw new InvalidOperationException("Invalid token.");

        return Task.FromResult(new GoogleTokenPayload(EmailToReturn));
    }
}