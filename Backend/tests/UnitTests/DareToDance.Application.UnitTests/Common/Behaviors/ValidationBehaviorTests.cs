using DareToDance.Application.Common.Behaviors;
using DareToDance.Application.Services.Authentication;
using DareToDance.Application.Services.Authentication.Commands.Register;
using ErrorOr;
using MediatR;

namespace DareToDance.Application.UnitTests.Common.Behaviors;

public class ValidationBehaviorTests
{
    private bool _nextCalled;

    private Task<ErrorOr<OtpChallengeResult>> Next(CancellationToken cancellationToken = default)
    {
        _nextCalled = true;
        return Task.FromResult<ErrorOr<OtpChallengeResult>>(OtpChallengeResult.CodeSent);
    }

    [Fact]
    public async Task Handle_WithoutValidator_PassesThrough()
    {
        var behavior = new ValidationBehavior<RegisterCommand, ErrorOr<OtpChallengeResult>>();

        var result = await behavior.Handle(new RegisterCommand("", "", "not-an-email"), Next, default);

        Assert.True(_nextCalled);
        Assert.False(result.IsError);
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShortCircuitsWithValidationErrors()
    {
        var behavior = new ValidationBehavior<RegisterCommand, ErrorOr<OtpChallengeResult>>(
            new RegisterCommandValidator());

        var result = await behavior.Handle(new RegisterCommand("", "Andric", "not-an-email"), Next, default);

        Assert.False(_nextCalled);
        Assert.True(result.IsError);
        Assert.All(result.Errors, error => Assert.Equal(ErrorType.Validation, error.Type));
    }

    [Fact]
    public async Task Handle_ValidRequest_ReachesTheHandler()
    {
        var behavior = new ValidationBehavior<RegisterCommand, ErrorOr<OtpChallengeResult>>(
            new RegisterCommandValidator());

        var result = await behavior.Handle(new RegisterCommand("Nikola", "Andric", "nikola@test.com"), Next, default);

        Assert.True(_nextCalled);
        Assert.False(result.IsError);
    }
}
