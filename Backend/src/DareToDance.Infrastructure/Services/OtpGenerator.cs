using System.Security.Cryptography;

namespace DareToDance.Infrastructure.Services;

internal sealed class OtpGenerator : IOtpGenerator
{
    public string Generate(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);

        Span<char> digits = stackalloc char[length];

        for (var i = 0; i < length; i++)
        {
            digits[i] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
        }

        return new string(digits);
    }
}
