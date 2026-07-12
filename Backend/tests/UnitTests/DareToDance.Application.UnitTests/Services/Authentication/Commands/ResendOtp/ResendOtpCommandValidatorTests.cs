using DareToDance.Application.Services.Authentication.Commands.ResendOtp;
using FluentValidation.TestHelper;

namespace DareToDance.Application.UnitTests.Services.Authentication.Commands.ResendOtp;

public class ResendOtpCommandValidatorTests
{
    private readonly ResendOtpCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidEmail_Passes()
    {
        var result = _validator.TestValidate(new ResendOtpCommand("nikola@test.com"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_BadEmail_Fails(string email)
    {
        var result = _validator.TestValidate(new ResendOtpCommand(email));

        result.ShouldHaveValidationErrorFor(command => command.Email);
    }
}
