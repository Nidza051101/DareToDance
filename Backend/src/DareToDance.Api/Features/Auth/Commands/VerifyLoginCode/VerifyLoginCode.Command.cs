using DareToDance.Api.Features.Auth.Shared;
using DareToDance.Domain.RefreshToken;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Options;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DareToDance.Api.Features.Auth.Commands.VerifyLoginCode;

public static partial class VerifyLoginCode
{
    public sealed record Command(string Recipient, string Code) : IRequest<ErrorOr<Result>>;

    public sealed record Result(
        User User,
        string AccessToken,
        DateTime AccessTokenExpiresAtUtc,
        string RefreshToken,
        DateTime RefreshTokenExpiresAtUtc);

    public sealed class Handler(
        AppDbContext dbContext,
        IPasswordHasher passwordHasher,
        IOptions<OtpSettings> otpOptions,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenService refreshTokenService)
        : IRequestHandler<Command, ErrorOr<Result>>
    {
        public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken cancellationToken)
        {
            var user = await FindUserAsync(command.Recipient, cancellationToken);

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

            var (accessToken, accessTokenExpiresAtUtc) = jwtTokenGenerator.GenerateToken(user);

            var (rawRefreshToken, refreshTokenHash, refreshTokenExpiresAtUtc) = refreshTokenService.Generate(utcNow);
            var refreshToken = RefreshToken.Create(user.Id, refreshTokenHash, utcNow, refreshTokenExpiresAtUtc);
            dbContext.RefreshTokens.Add(refreshToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            return new Result(user, accessToken, accessTokenExpiresAtUtc, rawRefreshToken, refreshTokenExpiresAtUtc);
        }

        // Recipient moze biti email ili telefon - trazimo korisnika po bilo kom od njih.
        private Task<User?> FindUserAsync(string recipient, CancellationToken cancellationToken)
        {
            var trimmed = recipient.Trim();
            var normalizedEmail = trimmed.ToLowerInvariant();
            var normalizedPhone = User.NormalizePhone(trimmed);

            return dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail || u.Phone == normalizedPhone, cancellationToken);
        }
    }
}
