using ErrorOr;

namespace DareToDance.Infrastructure.Services;

public interface IGoogleTokenVerifier
{
    Task<ErrorOr<GoogleIdentity>> VerifyAsync(string idToken, CancellationToken cancellationToken);
}

public sealed record GoogleIdentity(string Email, string FirstName, string LastName)
{
    public override string ToString()
        => $"GoogleIdentity {{ Email = {Email}, FirstName = [REDACTED], LastName = [REDACTED] }}";
}
