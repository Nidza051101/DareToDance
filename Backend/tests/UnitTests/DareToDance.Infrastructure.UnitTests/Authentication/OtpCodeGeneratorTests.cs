using DareToDance.Application.Services.Authentication.Otp;
using DareToDance.Infrastructure.Authentication;
using Microsoft.Extensions.Options;

namespace DareToDance.Infrastructure.UnitTests.Authentication;

public class OtpCodeGeneratorTests
{
    private static OtpCodeGenerator GeneratorWithLength(int codeLength) =>
        new(Options.Create(new OtpSettings
        {
            CodeLength = codeLength,
            ExpiryMinutes = 5,
            MaxFailedAttempts = 5,
            ResendCooldownSeconds = 60,
        }));

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    public void Generate_ProducesCodeOfConfiguredLength(int codeLength)
    {
        var generated = GeneratorWithLength(codeLength).Generate();

        Assert.Equal(codeLength, generated.Code.Length);
    }

    [Fact]
    public void Generate_ProducesOnlyDigits()
    {
        var generated = GeneratorWithLength(6).Generate();

        Assert.All(generated.Code, character => Assert.InRange(character, '0', '9'));
    }

    [Fact]
    public void Matches_ReturnsTrueForTheGeneratedCode()
    {
        var generated = GeneratorWithLength(6).Generate();

        Assert.True(GeneratorWithLength(6).Matches(generated.Code, generated.CodeHash));
    }

    [Fact]
    public void Matches_ReturnsFalseForADifferentCode()
    {
        var generator = GeneratorWithLength(6);
        var generated = generator.Generate();
        var different = generated.Code == "000000" ? "999999" : "000000";

        Assert.False(generator.Matches(different, generated.CodeHash));
    }

    [Fact]
    public void Generate_DoesNotStoreTheRawCodeInTheHash()
    {
        var generated = GeneratorWithLength(6).Generate();

        Assert.DoesNotContain(generated.Code, generated.CodeHash);
    }

    [Fact]
    public void Generate_ProducesVaryingCodes()
    {
        var generator = GeneratorWithLength(6);
        var codes = Enumerable.Range(0, 50).Select(_ => generator.Generate().Code).Distinct().Count();

        Assert.True(codes > 1, "50 generated codes were all identical — randomness is broken");
    }
}
