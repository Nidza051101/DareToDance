using DareToDance.Application.Services.Authentication.Commands.InitiateLogin;
using FluentValidation.TestHelper;

namespace DareToDance.Application.UnitTests.Services.Authentication.Commands.InitiateLogin;

public class InitiateLoginCommandValidatorTests
{
    private readonly InitiateLoginCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidEmail_Passes()
    {
        var result = _validator.TestValidate(new InitiateLoginCommand("nikola@test.com"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_BadEmail_Fails(string email)
    {
        var result = _validator.TestValidate(new InitiateLoginCommand(email));

        result.ShouldHaveValidationErrorFor(command => command.Email);
    }
}
