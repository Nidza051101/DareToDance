using System.Security.Cryptography;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Options;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;

namespace DareToDance.Api.Features.Auth.Shared;

// Zajednicka logika za slanje login koda - email i telefon flow su identicni
// osim nacina slanja: provera cooldown-a, generisanje koda, hesiranje, cuvanje
// na User-u sa rokom trajanja. Kanal slanja (email/SMS) se prosledjuje kao
// delegat sa mesta poziva (kompozicija), umesto nasledjivanja/Template Method-a -
// tako se odmah na mestu poziva vidi koji servis salje kod, bez skakanja u baznu klasu.
public static class RequestLoginCodeHelper
{
    public static async Task<ErrorOr<Success>> RequestCodeAsync(
        AppDbContext dbContext,
        IPasswordHasher passwordHasher,
        OtpSettings otpSettings,
        User user,
        string recipient,
        Func<string, string, CancellationToken, Task> sendCodeAsync,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        if (!user.IsActive)
        {
            // Isti generički uspeh kao i "korisnik ne postoji" - ne otkrivamo stanje naloga.
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

        await sendCodeAsync(recipient, code, cancellationToken);

        return Result.Success;
    }

    private static string GenerateCode(int length)
    {
        var max = (int)Math.Pow(10, length);
        var value = RandomNumberGenerator.GetInt32(0, max);

        return value.ToString(new string('0', length));
    }
}
