using DareToDance.Application.Services.Authentication.Commands.InitiateLogin;
using DareToDance.Application.Services.Authentication.Otp;
using DareToDance.Application.UnitTests.TestUtils;
using DareToDance.Domain.Entities;

namespace DareToDance.Application.UnitTests.Services.Authentication.Commands.InitiateLogin;

public class InitiateLoginCommandHandlerTests
{
    private const string KnownEmail = "dancer@test.com";

    private readonly FakeUserRepository _userRepository = new();
    private readonly CapturingEmailSender _emailSender = new();
    private readonly FakeDateTimeProvider _clock = new();
    private readonly InitiateLoginCommandHandler _handler;

    public InitiateLoginCommandHandlerTests()
    {
        var issuer = new OtpIssuer(
            new FakeOtpRepository(),
            new FakeOtpCodeGenerator(),
            _emailSender,
            _clock,
            TestOtpSettings.Default);

        _handler = new InitiateLoginCommandHandler(_userRepository, issuer);
        _userRepository.Add(new User { FirstName = "Test", LastName = "Dancer", Email = KnownEmail });
    }

    [Fact]
    public async Task Handle_KnownEmail_SendsCodeAndReturnsChallenge()
    {
        var result = await _handler.Handle(new InitiateLoginCommand(KnownEmail), default);

        Assert.False(result.IsError);
        var sent = Assert.Single(_emailSender.Sent);
        Assert.Equal(KnownEmail, sent.Email);
    }

    [Fact]
    public async Task Handle_UnknownEmail_SendsNothingButReturnsTheSameChallenge()
    {
        var known = await _handler.Handle(new InitiateLoginCommand(KnownEmail), default);
        var unknown = await _handler.Handle(new InitiateLoginCommand("ghost@test.com"), default);

        Assert.False(unknown.IsError);
        Assert.Equal(known.Value, unknown.Value);
        Assert.Single(_emailSender.Sent);
    }

    [Fact]
    public async Task Handle_WithinCooldown_SuppressesSendButStillReturnsChallenge()
    {
        await _handler.Handle(new InitiateLoginCommand(KnownEmail), default);
        _clock.Advance(TimeSpan.FromSeconds(10));

        var result = await _handler.Handle(new InitiateLoginCommand(KnownEmail), default);

        Assert.False(result.IsError);
        Assert.Single(_emailSender.Sent);
    }
}
