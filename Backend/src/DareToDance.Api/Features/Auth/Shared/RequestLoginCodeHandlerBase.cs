using System.Security.Cryptography;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Options;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace DareToDance.Api.Features.Auth.Shared;

// Template Method: the shared logic (cooldown check, code generation, storing it
// with an expiry - directly on User, no separate table) lives here. How the code
// is sent (email/SMS) is the variable part, left to the concrete handlers via SendCodeAsync.
public abstract class RequestLoginCodeHandlerBase(
    AppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IOptions<OtpSettings> otpOptions)
{
    protected async Task<ErrorOr<Success>> RequestCodeAsync(
        User user,
        string recipient,
        CancellationToken cancellationToken)
    {
        var otpSettings = otpOptions.Value;
        var utcNow = DateTime.UtcNow;

        if (!user.IsActive)
        {
            // Same generic success as "user not found" - don't reveal account state.
            return Result.Success;
        }

        if (user.LoginCodeCreatedAtUtc is not null &&
            utcNow < user.LoginCodeCreatedAtUtc.Value.AddSeconds(otpSettings.ResendCooldownSeconds))
        {
            return AuthErrors.CodeAlreadySent;
        }

        var code = GenerateCode(otpSettings.CodeLength);
        var codeHash = passwordHasher.Hash(code);
        var expiresAtUtc = utcNow.AddSeconds(otpSettings.ExpirySeconds);

        user.SetLoginCode(codeHash, expiresAtUtc, utcNow);

        await dbContext.SaveChangesAsync(cancellationToken);

        await SendCodeAsync(recipient, code, cancellationToken);

        return Result.Success;
    }

    protected abstract Task SendCodeAsync(string recipient, string code, CancellationToken cancellationToken);

    private static string GenerateCode(int length)
    {
        var max = (int)Math.Pow(10, length);
        var value = RandomNumberGenerator.GetInt32(0, max);

        return value.ToString(new string('0', length));
    }
}
