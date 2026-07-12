using DareToDance.Application.Common.Persistence;
using DareToDance.Application.Common.Services;
using DareToDance.Domain.Common.Errors;
using DareToDance.Domain.Entities;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace DareToDance.Application.Services.Authentication.Otp;

public class OtpIssuer(
    IOtpRepository otpRepository,
    IOtpCodeGenerator otpCodeGenerator,
    IEmailSender emailSender,
    IDateTimeProvider dateTimeProvider,
    IOptions<OtpSettings> otpOptions)
{
    private readonly OtpSettings _settings = otpOptions.Value;

    public async Task<ErrorOr<Success>> IssueAsync(User user, OtpPurpose purpose, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var existing = otpRepository.GetLatestByUserId(user.Id, purpose);

        if (existing is not null)
        {
            if (now < existing.CreatedAt.AddSeconds(_settings.ResendCooldownSeconds))
            {
                return Errors.Otp.ResendCooldown;
            }

            existing.IsConsumed = true;
            otpRepository.Update(existing);
        }

        var generated = otpCodeGenerator.Generate();

        otpRepository.Add(new OtpCode
        {
            UserId = user.Id,
            CodeHash = generated.CodeHash,
            Purpose = purpose,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(_settings.ExpiryMinutes),
        });

        await emailSender.SendOtpAsync(user.Email, generated.Code, cancellationToken);

        return Result.Success;
    }
}
