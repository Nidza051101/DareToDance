using DareToDance.Application.Services.Authentication.Otp;
using DareToDance.Application.UnitTests.TestUtils;
using DareToDance.Domain.Entities;

namespace DareToDance.Application.UnitTests.Services.Authentication.Otp;

public class OtpIssuerTests
{
    private readonly FakeOtpRepository _otpRepository = new();
    private readonly FakeOtpCodeGenerator _otpCodeGenerator = new();
    private readonly CapturingEmailSender _emailSender = new();
    private readonly FakeDateTimeProvider _clock = new();
    private readonly OtpIssuer _issuer;
    private readonly User _user = new() { FirstName = "Test", LastName = "Dancer", Email = "dancer@test.com" };

    public OtpIssuerTests()
    {
        _issuer = new OtpIssuer(
            _otpRepository,
            _otpCodeGenerator,
            _emailSender,
            _clock,
            TestOtpSettings.Default);
    }

    [Fact]
    public async Task IssueAsync_StoresHashOfCode_AndEmailsTheRawCode()
    {
        await _issuer.IssueAsync(_user, OtpPurpose.Login, default);

        var stored = Assert.Single(_otpRepository.Codes);
        var sent = Assert.Single(_emailSender.Sent);

        Assert.Equal(_user.Email, sent.Email);
        Assert.Equal(FakeOtpCodeGenerator.HashOf(sent.Code), stored.CodeHash);
        Assert.NotEqual(sent.Code, stored.CodeHash);
    }

    [Fact]
    public async Task IssueAsync_SetsTimestampsFromClockAndSettings()
    {
        await _issuer.IssueAsync(_user, OtpPurpose.Login, default);

        var stored = Assert.Single(_otpRepository.Codes);
        Assert.Equal(_clock.UtcNow, stored.CreatedAt);
        Assert.Equal(_clock.UtcNow.AddMinutes(5), stored.ExpiresAt);
    }

    [Fact]
    public async Task IssueAsync_WithinCooldown_ReturnsError_StoresNothing_SendsNothing()
    {
        await _issuer.IssueAsync(_user, OtpPurpose.Login, default);
        _clock.Advance(TimeSpan.FromSeconds(30));

        var result = await _issuer.IssueAsync(_user, OtpPurpose.Login, default);

        Assert.True(result.IsError);
        Assert.Equal("Otp.ResendCooldown", result.FirstError.Code);
        Assert.Single(_otpRepository.Codes);
        Assert.Single(_emailSender.Sent);
    }

    [Fact]
    public async Task IssueAsync_AfterCooldown_InvalidatesOldCodeAndIssuesNew()
    {
        await _issuer.IssueAsync(_user, OtpPurpose.Login, default);
        var first = _otpRepository.Codes[0];
        _clock.Advance(TimeSpan.FromSeconds(60));

        var result = await _issuer.IssueAsync(_user, OtpPurpose.Login, default);

        Assert.False(result.IsError);
        Assert.True(first.IsConsumed);
        Assert.Equal(2, _otpRepository.Codes.Count);
        Assert.Equal(2, _emailSender.Sent.Count);
    }

    [Fact]
    public async Task IssueAsync_ConsumedCodeDoesNotTriggerCooldown()
    {
        await _issuer.IssueAsync(_user, OtpPurpose.Login, default);
        _otpRepository.Codes[0].IsConsumed = true;
        _clock.Advance(TimeSpan.FromSeconds(1));

        var result = await _issuer.IssueAsync(_user, OtpPurpose.Login, default);

        Assert.False(result.IsError);
        Assert.Equal(2, _otpRepository.Codes.Count);
    }

    [Fact]
    public async Task IssueAsync_GeneratesFreshCodeEachTime()
    {
        await _issuer.IssueAsync(_user, OtpPurpose.Login, default);
        _clock.Advance(TimeSpan.FromSeconds(60));
        await _issuer.IssueAsync(_user, OtpPurpose.Login, default);

        Assert.NotEqual(_emailSender.Sent[0].Code, _emailSender.Sent[1].Code);
    }
}
