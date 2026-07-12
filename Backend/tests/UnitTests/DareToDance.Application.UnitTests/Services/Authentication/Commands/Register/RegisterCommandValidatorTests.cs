using DareToDance.Application.Services.Authentication.Commands.Register;
using FluentValidation.TestHelper;

namespace DareToDance.Application.UnitTests.Services.Authentication.Commands.Register;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var result = _validator.TestValidate(new RegisterCommand("Nikola", "Andric", "nikola@test.com"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyFirstName_Fails()
    {
        var result = _validator.TestValidate(new RegisterCommand("", "Andric", "nikola@test.com"));

        result.ShouldHaveValidationErrorFor(command => command.FirstName);
    }

    [Fact]
    public void Validate_EmptyLastName_Fails()
    {
        var result = _validator.TestValidate(new RegisterCommand("Nikola", "", "nikola@test.com"));

        result.ShouldHaveValidationErrorFor(command => command.LastName);
    }

    [Fact]
    public void Validate_OverlongName_Fails()
    {
        var result = _validator.TestValidate(new RegisterCommand(new string('a', 101), "Andric", "nikola@test.com"));

        result.ShouldHaveValidationErrorFor(command => command.FirstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_BadEmail_Fails(string email)
    {
        var result = _validator.TestValidate(new RegisterCommand("Nikola", "Andric", email));

        result.ShouldHaveValidationErrorFor(command => command.Email);
    }
}
