using System.Security.Cryptography;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Options;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace DareToDance.Api.Features.Auth.Shared;

// Template Method: zajednicka logika (cooldown provera, generisanje koda, cuvanje
// sa rokom trajanja - direktno na User, bez posebne tabele) je ovde. Nacin slanja
// koda (email/sms) je promenljiv deo, prepusten konkretnim handlerima kroz SendCodeAsync.
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
