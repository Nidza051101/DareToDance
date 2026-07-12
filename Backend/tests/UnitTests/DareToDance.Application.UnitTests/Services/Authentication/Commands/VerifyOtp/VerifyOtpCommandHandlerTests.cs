using DareToDance.Application.Services.Authentication.Commands.VerifyOtp;
using DareToDance.Application.UnitTests.TestUtils;
using DareToDance.Domain.Entities;

namespace DareToDance.Application.UnitTests.Services.Authentication.Commands.VerifyOtp;

public class VerifyOtpCommandHandlerTests
{
    private const string Email = "dancer@test.com";
    private const string Code = "123456";
    private const string WrongCode = "654321";

    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeOtpRepository _otpRepository = new();
    private readonly FakeJwtTokenGenerator _jwtTokenGenerator = new();
    private readonly FakeDateTimeProvider _clock = new();
    private readonly VerifyOtpCommandHandler _handler;
    private readonly User _user;

    public VerifyOtpCommandHandlerTests()
    {
        _handler = new VerifyOtpCommandHandler(
            _userRepository,
            _otpRepository,
            new FakeOtpCodeGenerator(),
            _jwtTokenGenerator,
            _clock,
            TestOtpSettings.Default);

        _user = new User { FirstName = "Test", LastName = "Dancer", Email = Email };
        _userRepository.Add(_user);
    }

    private OtpCode AddActiveOtp(string code = Code, int failedAttempts = 0, bool isConsumed = false)
    {
        var otp = new OtpCode
        {
            UserId = _user.Id,
            CodeHash = FakeOtpCodeGenerator.HashOf(code),
            Purpose = OtpPurpose.Login,
            CreatedAt = _clock.UtcNow,
            ExpiresAt = _clock.UtcNow.AddMinutes(5),
            FailedAttempts = failedAttempts,
            IsConsumed = isConsumed,
        };
        _otpRepository.Add(otp);
        return otp;
    }

    [Fact]
    public async Task Handle_WithCorrectCode_ReturnsTokenAndConsumesCode()
    {
        var otp = AddActiveOtp();

        var result = await _handler.Handle(new VerifyOtpCommand(Email, Code), default);

        Assert.False(result.IsError);
        Assert.Equal($"token-for-{_user.Id}", result.Value.Token);
        Assert.Equal(Email, result.Value.Email);
        Assert.True(otp.IsConsumed);
        Assert.Equal(Email, _jwtTokenGenerator.LastSubject!.Email);
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ReturnsInvalidCode()
    {
        AddActiveOtp();

        var result = await _handler.Handle(new VerifyOtpCommand("ghost@test.com", Code), default);

        Assert.True(result.IsError);
        Assert.Equal("Otp.InvalidCode", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_WithNoActiveCode_ReturnsInvalidCode()
    {
        var result = await _handler.Handle(new VerifyOtpCommand(Email, Code), default);

        Assert.True(result.IsError);
        Assert.Equal("Otp.InvalidCode", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_WithConsumedCode_ReturnsInvalidCode()
    {
        AddActiveOtp(isConsumed: true);

        var result = await _handler.Handle(new VerifyOtpCommand(Email, Code), default);

        Assert.True(result.IsError);
        Assert.Equal("Otp.InvalidCode", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_WithExpiredCode_ReturnsExpired()
    {
        AddActiveOtp();
        _clock.Advance(TimeSpan.FromMinutes(6));

        var result = await _handler.Handle(new VerifyOtpCommand(Email, Code), default);

        Assert.True(result.IsError);
        Assert.Equal("Otp.Expired", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_AtExactExpiryInstant_ReturnsExpired()
    {
        AddActiveOtp();
        _clock.Advance(TimeSpan.FromMinutes(5));

        var result = await _handler.Handle(new VerifyOtpCommand(Email, Code), default);

        Assert.True(result.IsError);
        Assert.Equal("Otp.Expired", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_WithWrongCode_ReturnsInvalidCodeAndIncrementsAttemptsWithoutConsuming()
    {
        var otp = AddActiveOtp();

        var result = await _handler.Handle(new VerifyOtpCommand(Email, WrongCode), default);

        Assert.True(result.IsError);
        Assert.Equal("Otp.InvalidCode", result.FirstError.Code);
        Assert.Equal(1, otp.FailedAttempts);
        Assert.False(otp.IsConsumed);
    }

    [Fact]
    public async Task Handle_AfterFourWrongAttempts_CorrectCodeStillSucceeds()
    {
        AddActiveOtp(failedAttempts: 4);

        var result = await _handler.Handle(new VerifyOtpCommand(Email, Code), default);

        Assert.False(result.IsError);
    }

    [Fact]
    public async Task Handle_AfterMaxFailedAttempts_CorrectCodeReturnsTooManyAttempts()
    {
        AddActiveOtp(failedAttempts: 5);

        var result = await _handler.Handle(new VerifyOtpCommand(Email, Code), default);

        Assert.True(result.IsError);
        Assert.Equal("Otp.TooManyAttempts", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_FiveWrongGuesses_LockOutTheCorrectCode()
    {
        AddActiveOtp();

        for (var i = 0; i < 5; i++)
        {
            await _handler.Handle(new VerifyOtpCommand(Email, WrongCode), default);
        }

        var result = await _handler.Handle(new VerifyOtpCommand(Email, Code), default);

        Assert.True(result.IsError);
        Assert.Equal("Otp.TooManyAttempts", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_OnAnyFailure_DoesNotIssueToken()
    {
        AddActiveOtp();

        await _handler.Handle(new VerifyOtpCommand(Email, WrongCode), default);

        Assert.Null(_jwtTokenGenerator.LastSubject);
    }
}
