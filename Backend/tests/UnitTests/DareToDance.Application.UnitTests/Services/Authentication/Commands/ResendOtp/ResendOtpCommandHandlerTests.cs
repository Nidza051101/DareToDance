using DareToDance.Application.Services.Authentication.Commands.ResendOtp;
using DareToDance.Application.Services.Authentication.Otp;
using DareToDance.Application.UnitTests.TestUtils;
using DareToDance.Domain.Entities;

namespace DareToDance.Application.UnitTests.Services.Authentication.Commands.ResendOtp;

public class ResendOtpCommandHandlerTests
{
    private const string KnownEmail = "dancer@test.com";

    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeOtpRepository _otpRepository = new();
    private readonly CapturingEmailSender _emailSender = new();
    private readonly FakeDateTimeProvider _clock = new();
    private readonly ResendOtpCommandHandler _handler;

    public ResendOtpCommandHandlerTests()
    {
        var issuer = new OtpIssuer(
            _otpRepository,
            new FakeOtpCodeGenerator(),
            _emailSender,
            _clock,
            TestOtpSettings.Default);

        _handler = new ResendOtpCommandHandler(_userRepository, issuer);
        _userRepository.Add(new User { FirstName = "Test", LastName = "Dancer", Email = KnownEmail });
    }

    [Fact]
    public async Task Handle_UnknownEmail_ReturnsGenericChallengeAndSendsNothing()
    {
        var result = await _handler.Handle(new ResendOtpCommand("ghost@test.com"), default);

        Assert.False(result.IsError);
        Assert.Empty(_emailSender.Sent);
    }

    [Fact]
    public async Task Handle_WithinCooldown_ReturnsResendCooldown()
    {
        await _handler.Handle(new ResendOtpCommand(KnownEmail), default);
        _clock.Advance(TimeSpan.FromSeconds(30));

        var result = await _handler.Handle(new ResendOtpCommand(KnownEmail), default);

        Assert.True(result.IsError);
        Assert.Equal("Otp.ResendCooldown", result.FirstError.Code);
        Assert.Single(_emailSender.Sent);
    }

    [Fact]
    public async Task Handle_AfterCooldown_InvalidatesOldCodeAndSendsNew()
    {
        await _handler.Handle(new ResendOtpCommand(KnownEmail), default);
        var first = _otpRepository.Codes[0];
        _clock.Advance(TimeSpan.FromSeconds(60));

        var result = await _handler.Handle(new ResendOtpCommand(KnownEmail), default);

        Assert.False(result.IsError);
        Assert.True(first.IsConsumed);
        Assert.Equal(2, _emailSender.Sent.Count);
    }
}
