using DareToDance.Domain.RefreshToken.Id;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DareToDance.Api.Features.Auth.Commands.Logout;

public static class Logout
{
    public sealed record Command(string RefreshToken) : IRequest<ErrorOr<Success>>
    {
        public override string ToString()
            => "Logout.Command { RefreshToken = [REDACTED] }";
    }

    // Logout is silent on purpose: live, rotated, expired, and garbage tokens
    // all produce the same 204, because any distinct response would reveal
    // which sessions exist. Possession of the secret is the logout capability
    // — no state check gates the revocation, so even a stale rotated token
    // still kills its own session.
    public sealed class Handler(
        AppDbContext dbContext,
        IRefreshTokenHasher refreshTokenHasher,
        TimeProvider timeProvider,
        ILogger<Handler> logger)
        : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var utcNow = timeProvider.GetUtcNow().UtcDateTime;

            if (!RefreshTokenWireFormat.TryParse(command.RefreshToken, out var parsedId, out var secret))
            {
                await EqualizeTimingAsync(cancellationToken);
                return Result.Success;
            }

            var tokenId = RefreshTokenId.Create(parsedId);

            var token = await dbContext.RefreshTokens
                .AsNoTracking()
                .SingleOrDefaultAsync(t => t.Id == tokenId, cancellationToken);

            if (token is null)
            {
                DummyHash();
                return Result.Success;
            }

            if (!refreshTokenHasher.Verify(token.TokenHash, token.Id.Value, secret))
            {
                return Result.Success;
            }

            // ExecuteUpdate, mirroring the reuse sweep in Refresh: untracked,
            // xmin-safe, and covers the whole family including rows this
            // handler never loaded.
            await dbContext.RefreshTokens
                .Where(t => t.FamilyId == token.FamilyId && t.RevokedAtUtc == null)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(t => t.RevokedAtUtc, utcNow),
                    cancellationToken);

            logger.LogInformation(
                "SessionRevoked: family {FamilyId} for user {UserId}",
                token.FamilyId,
                token.UserId.Value);

            return Result.Success;
        }

        private async Task EqualizeTimingAsync(CancellationToken cancellationToken)
        {
            var throwawayId = RefreshTokenId.Create(Guid.NewGuid());

            await dbContext.RefreshTokens
                .AsNoTracking()
                .SingleOrDefaultAsync(t => t.Id == throwawayId, cancellationToken);

            DummyHash();
        }

        private void DummyHash()
            => refreshTokenHasher.Hash(Guid.NewGuid(), "timing-equalization-dummy-secret");
    }
}
