using DareToDance.Api.Features.Auth.Shared;
using DareToDance.Domain.RefreshToken;
using DareToDance.Domain.User.Id;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DareToDance.Api.Features.Auth.Commands.RefreshAccessToken;

public static partial class RefreshAccessToken
{
    public sealed record Command(string RefreshToken) : IRequest<ErrorOr<Result>>;

    public sealed record Result(
        string AccessToken,
        DateTime AccessTokenExpiresAtUtc,
        string RefreshToken,
        DateTime RefreshTokenExpiresAtUtc);

    public sealed class Handler(
        AppDbContext dbContext,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenService refreshTokenService)
        : IRequestHandler<Command, ErrorOr<Result>>
    {
        public async Task<ErrorOr<Result>> Handle(Command command, CancellationToken cancellationToken)
        {
            var utcNow = DateTime.UtcNow;
            var incomingTokenHash = refreshTokenService.Hash(command.RefreshToken);

            var existingToken = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == incomingTokenHash, cancellationToken);

            if (existingToken is null)
            {
                return AuthErrors.InvalidRefreshToken;
            }

            if (existingToken.RevokedAtUtc is not null)
            {
                if (existingToken.ReplacedByTokenId is not null)
                {
                    await RevokeAllActiveTokensAsync(existingToken.UserId, utcNow, cancellationToken);
                }

                return AuthErrors.InvalidRefreshToken;
            }

            if (existingToken.IsExpired(utcNow))
            {
                return AuthErrors.InvalidRefreshToken;
            }

            var user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == existingToken.UserId, cancellationToken);

            if (user is null || !user.IsActive)
            {
                return AuthErrors.InvalidRefreshToken;
            }

            var (newRawToken, newTokenHash, newExpiresAtUtc) = refreshTokenService.Generate(utcNow);
            var newRefreshToken = RefreshToken.Create(user.Id, newTokenHash, utcNow, newExpiresAtUtc);

            existingToken.Revoke(utcNow, newRefreshToken.Id);
            dbContext.RefreshTokens.Add(newRefreshToken);

            var (accessToken, accessTokenExpiresAtUtc) = jwtTokenGenerator.GenerateToken(user);

            await dbContext.SaveChangesAsync(cancellationToken);

            return new Result(accessToken, accessTokenExpiresAtUtc, newRawToken, newExpiresAtUtc);
        }

        private async Task RevokeAllActiveTokensAsync(UserId userId, DateTime utcNow, CancellationToken cancellationToken)
        {
            var activeTokens = await dbContext.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
            {
                token.Revoke(utcNow);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
