using DareToDance.Application.Services.Authentication.Commands.VerifyOtp;
using FluentValidation.TestHelper;

namespace DareToDance.Application.UnitTests.Services.Authentication.Commands.VerifyOtp;

public class VerifyOtpCommandValidatorTests
{
    private readonly VerifyOtpCommandValidator _validator = new();

    [Theory]
    [InlineData("123456")]
    [InlineData("1234")]
    [InlineData("1234567890")]
    public void Validate_NumericCodeWithinLengthRange_Passes(string code)
    {
        var result = _validator.TestValidate(new VerifyOtpCommand("nikola@test.com", code));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("12345678901")]
    [InlineData("12a456")]
    [InlineData("abcdef")]
    public void Validate_MalformedCode_Fails(string code)
    {
        var result = _validator.TestValidate(new VerifyOtpCommand("nikola@test.com", code));

        result.ShouldHaveValidationErrorFor(command => command.Code);
    }

    [Fact]
    public void Validate_BadEmail_Fails()
    {
        var result = _validator.TestValidate(new VerifyOtpCommand("not-an-email", "123456"));

        result.ShouldHaveValidationErrorFor(command => command.Email);
    }
}
