using DareToDance.Api.Features.Auth.Shared;
using DareToDance.Domain.RefreshToken.Id;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DareToDance.Api.Features.Auth.Commands.Refresh;

// Named Refresh (not RefreshToken) so the slice never collides with the
// Domain.RefreshToken aggregate in usings.
public static class Refresh
{
    public sealed record Command(string RefreshToken) : IRequest<ErrorOr<AuthResult>>
    {
        public override string ToString()
            => "Refresh.Command { RefreshToken = [REDACTED] }";
    }

    // Every failure — malformed wire token, unknown id, wrong secret, reuse,
    // expiry — returns the same AuthErrors.InvalidToken, and every failure
    // path performs one lookup and one digest, so neither the response nor
    // its timing reveals session state.
    public sealed class Handler(
        AppDbContext dbContext,
        IRefreshTokenHasher refreshTokenHasher,
        ITokenService tokenService,
        TimeProvider timeProvider,
        ILogger<Handler> logger)
        : IRequestHandler<Command, ErrorOr<AuthResult>>
    {
        public async Task<ErrorOr<AuthResult>> Handle(Command command, CancellationToken cancellationToken)
        {
            var utcNow = timeProvider.GetUtcNow().UtcDateTime;

            if (!RefreshTokenWireFormat.TryParse(command.RefreshToken, out var parsedId, out var secret))
            {
                await EqualizeTimingAsync(cancellationToken);
                return AuthErrors.InvalidToken;
            }

            var tokenId = RefreshTokenId.Create(parsedId);

            var token = await dbContext.RefreshTokens
                .SingleOrDefaultAsync(t => t.Id == tokenId, cancellationToken);

            if (token is null)
            {
                DummyHash();
                return AuthErrors.InvalidToken;
            }

            // Secret before state: only a caller holding a genuinely issued
            // token may influence session state. Without this order, anyone
            // who ever saw a token id (logs, traces) could kill the family
            // by presenting the id with a garbage secret.
            if (!refreshTokenHasher.Verify(token.TokenHash, token.Id.Value, secret))
            {
                logger.LogWarning(
                    "RefreshSecretMismatch: token {TokenId} for user {UserId}",
                    token.Id.Value,
                    token.UserId.Value);

                return AuthErrors.InvalidToken;
            }

            if (token.ConsumedAtUtc is not null || token.RevokedAtUtc is not null)
            {
                // A correct secret on a dead token means a stale copy exists —
                // assume theft and kill every token in the session. Checked
                // before expiry: replaying an expired-but-rotated ancestor is
                // still theft evidence while its descendants may be live.
                await RevokeFamilyAsync(token.FamilyId, utcNow, cancellationToken);

                logger.LogWarning(
                    "RefreshTokenReuseDetected: token {TokenId} family {FamilyId} for user {UserId}",
                    token.Id.Value,
                    token.FamilyId,
                    token.UserId.Value);

                return AuthErrors.InvalidToken;
            }

            if (utcNow >= token.ExpiresAtUtc)
            {
                // No writes: an expired live token just means the session died
                // of inactivity, and dead rows are the purge job's problem.
                return AuthErrors.InvalidToken;
            }

            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == token.UserId, cancellationToken);

            if (user is null)
            {
                return AuthErrors.InvalidToken;
            }

            // Consumes the predecessor and creates the successor; one
            // SaveChanges commits both atomically.
            var successorResult = tokenService.RotateRefreshToken(token, utcNow);

            if (successorResult.IsError)
            {
                // Unreachable after the state and expiry gates above — a
                // tripped invariant is a bug and must surface as a 500, not
                // be disguised as an auth failure.
                return successorResult.Errors;
            }

            var successor = successorResult.Value;
            dbContext.RefreshTokens.Add(successor.Token);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Two parallel refreshes of the same token: both read it live,
                // the xmin race loser lands here. A photo-finish race is not
                // theft evidence, so the family stays alive — the winner's
                // rotation stands.
                return AuthErrors.InvalidToken;
            }

            var accessToken = tokenService.CreateAccessToken(user);

            logger.LogInformation(
                "TokenRefreshed: token {TokenId} -> {SuccessorId} family {FamilyId} for user {UserId}",
                token.Id.Value,
                successor.Token.Id.Value,
                token.FamilyId,
                user.Id.Value);

            return new AuthResult(
                user,
                accessToken,
                successor.WireToken,
                successor.Token.ExpiresAtUtc);
        }

        private async Task RevokeFamilyAsync(Guid familyId, DateTime utcNow, CancellationToken cancellationToken)
        {
            // ExecuteUpdate (not tracked entities): the sweep must not race the
            // xmin token into a 500, and it revokes rows this handler never
            // loaded. A successor inserted by an in-flight parallel rotation
            // can escape the sweep — accepted micro-race; the attacker still
            // never receives a token.
            await dbContext.RefreshTokens
                .Where(t => t.FamilyId == familyId && t.RevokedAtUtc == null)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(t => t.RevokedAtUtc, utcNow),
                    cancellationToken);
        }

        // Mirror the real path's lookup and digest work for tokens that never
        // reach them, so the cheap failures are not obviously faster.
        private async Task EqualizeTimingAsync(CancellationToken cancellationToken)
        {
            var throwawayId = RefreshTokenId.Create(Guid.NewGuid());

            await dbContext.RefreshTokens
                .SingleOrDefaultAsync(t => t.Id == throwawayId, cancellationToken);

            DummyHash();
        }

        private void DummyHash()
            => refreshTokenHasher.Hash(Guid.NewGuid(), "timing-equalization-dummy-secret");
    }
}
