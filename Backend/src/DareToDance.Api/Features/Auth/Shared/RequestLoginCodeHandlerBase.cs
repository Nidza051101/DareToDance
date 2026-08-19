using System.Security.Cryptography;
using DareToDance.Domain.LoginCode;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Options;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DareToDance.Api.Features.Auth.Shared;

// Template Method: zajednicka logika (cooldown provera, generisanje koda, cuvanje
// sa rokom trajanja) je ovde. Nacin slanja koda (email/sms) je promenljiv deo,
// prepusten konkretnim handlerima kroz SendCodeAsync.
public abstract class RequestLoginCodeHandlerBase(
    AppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IOptions<OtpSettings> otpOptions)
{
    protected async Task<ErrorOr<Success>> RequestCodeAsync(
        User user,
        LoginChannel channel,
        string recipient,
        CancellationToken cancellationToken)
    {
        var otpSettings = otpOptions.Value;
        var utcNow = DateTime.UtcNow;

        var lastCode = await dbContext.LoginCodes
            .Where(lc => lc.UserId == user.Id && lc.ConsumedAtUtc == null)
            .OrderByDescending(lc => lc.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastCode is not null &&
            utcNow < lastCode.CreatedAtUtc.AddSeconds(otpSettings.ResendCooldownSeconds))
        {
            return AuthErrors.CodeAlreadySent;
        }

        var code = GenerateCode(otpSettings.CodeLength);
        var codeHash = passwordHasher.Hash(code);
        var expiresAtUtc = utcNow.AddMinutes(otpSettings.ExpiryMinutes);

        var loginCode = LoginCode.Create(user.Id, channel, codeHash, expiresAtUtc);
        dbContext.LoginCodes.Add(loginCode);

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
