using DareToDance.Api.Features.Auth.Shared;
using DareToDance.Domain.User;
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
        var normalizedPhone = User.NormalizePhone(recipient);

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail || u.Phone == normalizedPhone, cancellationToken);

        var utcNow = DateTime.UtcNow;

        if (user is null || !user.IsActive || user.LoginCodeHash is null || !user.HasActiveLoginCode(utcNow))
        {
            return AuthErrors.InvalidCode;
        }

        if (!passwordHasher.Verify(user.LoginCodeHash, command.Code))
        {
            user.RegisterLoginCodeFailedAttempt(otpOptions.Value.MaxFailedAttempts, utcNow);
            await dbContext.SaveChangesAsync(cancellationToken);

            return user.Status == UserStatus.Blocked
                ? AuthErrors.AccountBlocked
                : AuthErrors.InvalidCode;
        }

        user.ClearLoginCode(utcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        var (accessToken, expiresAtUtc) = jwtTokenGenerator.GenerateToken(user);

        return new LoginResult(user, accessToken, expiresAtUtc);
    }
}
