using DareToDance.Api.Features.Auth.Shared;
using DareToDance.Infrastructure.Options;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DareToDance.Api.Features.Auth.Commands.VerifyLoginCode;

public sealed record VerifyLoginCodeCommand(string Recipient, string Code) : IRequest<ErrorOr<LoginResult>>;

public sealed class VerifyLoginCodeCommandHandler(
    AppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IOptions<OtpSettings> otpOptions,
    IJwtTokenGenerator jwtTokenGenerator)
    : IRequestHandler<VerifyLoginCodeCommand, ErrorOr<LoginResult>>
{
    public async Task<ErrorOr<LoginResult>> Handle(VerifyLoginCodeCommand command, CancellationToken cancellationToken)
    {
        var recipient = command.Recipient.Trim();
        var normalizedEmail = recipient.ToLowerInvariant();

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail || u.Phone == recipient, cancellationToken);

        if (user is null)
        {
            return AuthErrors.InvalidCode;
        }

        var loginCode = await dbContext.LoginCodes
            .Where(lc => lc.UserId == user.Id && lc.ConsumedAtUtc == null)
            .OrderByDescending(lc => lc.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var utcNow = DateTime.UtcNow;

        if (loginCode is null || loginCode.IsExpired(utcNow))
        {
            return AuthErrors.InvalidCode;
        }

        if (loginCode.FailedAttempts >= otpOptions.Value.MaxFailedAttempts)
        {
            return AuthErrors.TooManyAttempts;
        }

        if (!passwordHasher.Verify(loginCode.CodeHash, command.Code))
        {
            loginCode.RegisterFailedAttempt(utcNow);
            await dbContext.SaveChangesAsync(cancellationToken);

            return AuthErrors.InvalidCode;
        }

        loginCode.MarkConsumed(utcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        var (accessToken, expiresAtUtc) = jwtTokenGenerator.GenerateToken(user);

        return new LoginResult(user, accessToken, expiresAtUtc);
    }
}
