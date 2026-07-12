using DareToDance.Application.Services.Authentication.Commands.Register;
using DareToDance.Application.Services.Authentication.Otp;
using DareToDance.Application.UnitTests.TestUtils;
using DareToDance.Domain.Entities;

namespace DareToDance.Application.UnitTests.Services.Authentication.Commands.Register;

public class RegisterCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeOtpRepository _otpRepository = new();
    private readonly CapturingEmailSender _emailSender = new();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        var issuer = new OtpIssuer(
            _otpRepository,
            new FakeOtpCodeGenerator(),
            _emailSender,
            new FakeDateTimeProvider(),
            TestOtpSettings.Default);

        _handler = new RegisterCommandHandler(_userRepository, issuer);
    }

    [Fact]
    public async Task Handle_NewEmail_CreatesUserIssuesCodeAndReturnsChallenge()
    {
        var result = await _handler.Handle(new RegisterCommand("Nikola", "Andric", "nikola@test.com"), default);

        Assert.False(result.IsError);
        var user = Assert.Single(_userRepository.Users);
        Assert.Equal("Nikola", user.FirstName);
        Assert.Equal("Andric", user.LastName);
        Assert.Equal("nikola@test.com", user.Email);

        var sent = Assert.Single(_emailSender.Sent);
        Assert.Equal("nikola@test.com", sent.Email);

        var otp = Assert.Single(_otpRepository.Codes);
        Assert.Equal(user.Id, otp.UserId);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsDuplicateEmailAndSendsNothing()
    {
        _userRepository.Add(new User { FirstName = "First", LastName = "User", Email = "taken@test.com" });

        var result = await _handler.Handle(new RegisterCommand("Second", "User", "taken@test.com"), default);

        Assert.True(result.IsError);
        Assert.Equal("User.DuplicateEmail", result.FirstError.Code);
        Assert.Single(_userRepository.Users);
        Assert.Empty(_emailSender.Sent);
    }

    [Fact]
    public async Task Handle_Success_ReturnsChallengeNotAToken()
    {
        var result = await _handler.Handle(new RegisterCommand("Nikola", "Andric", "nikola@test.com"), default);

        Assert.False(result.IsError);
        Assert.Equal("If the email address is valid, a sign-in code has been sent.", result.Value.Message);
    }
}
