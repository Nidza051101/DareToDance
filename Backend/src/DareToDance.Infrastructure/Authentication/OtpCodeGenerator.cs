using System.Security.Cryptography;
using System.Text;
using DareToDance.Application.Services.Authentication.Otp;
using Microsoft.Extensions.Options;

namespace DareToDance.Infrastructure.Authentication;

public class OtpCodeGenerator(IOptions<OtpSettings> otpOptions) : IOtpCodeGenerator
{
    private readonly OtpSettings _settings = otpOptions.Value;

    public GeneratedOtp Generate()
    {
        var digits = new char[_settings.CodeLength];
        for (var i = 0; i < digits.Length; i++)
        {
            digits[i] = (char)('0' + RandomNumberGenerator.GetInt32(10));
        }

        var code = new string(digits);

        return new GeneratedOtp(code, Hash(code));
    }

    public bool Matches(string code, string codeHash)
    {
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(Hash(code)),
            Convert.FromHexString(codeHash));
    }

    private static string Hash(string code)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));
    }
}
