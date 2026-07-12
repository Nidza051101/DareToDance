using DareToDance.Application.Services.Authentication.Otp;
using Microsoft.Extensions.Options;

namespace DareToDance.Application.UnitTests.TestUtils;

public static class TestOtpSettings
{
    public static IOptions<OtpSettings> Default => Options.Create(new OtpSettings
    {
        CodeLength = 6,
        ExpiryMinutes = 5,
        MaxFailedAttempts = 5,
        ResendCooldownSeconds = 60,
    });
}
