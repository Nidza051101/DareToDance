using DareToDance.Api.Features.Auth.Shared;
using DareToDance.Domain.OtpChallenge;
using DareToDance.Domain.User.Id;
using DareToDance.Infrastructure.Options;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DareToDance.Api.Features.Auth.Commands.VerifyOtp;

public static class VerifyOtp
{
    public sealed record Command(string Email, string Code) : IRequest<ErrorOr<AuthResult>>
    {
        public override string ToString()
            => $"VerifyOtp.Command {{ Email = {Email}, Code = [REDACTED] }}";
    }

    // Every failure — unknown email, no live challenge, expired, attempt cap,
    // wrong code — returns the same AuthErrors.InvalidCode, and every failure
    // path performs one HMAC computation, so neither the response nor its
    // timing reveals account or challenge state.
    public sealed class Handler(
        AppDbContext dbContext,
        IOtpCodeHasher otpCodeHasher,
        ITokenService tokenService,
        IOptions<OtpSettings> otpOptions,
        TimeProvider timeProvider,
        ILogger<Handler> logger)
        : IRequestHandler<Command, ErrorOr<AuthResult>>
    {
        public async Task<ErrorOr<AuthResult>> Handle(Command command, CancellationToken cancellationToken)
        {
            var settings = otpOptions.Value;
            var utcNow = timeProvider.GetUtcNow().UtcDateTime;
            var email = AuthEmail.Normalize(command.Email);

            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user is null)
            {
                await EqualizeTimingAsync(command.Code, cancellationToken);
                return AuthErrors.InvalidCode;
            }

            // The partial unique index guarantees at most one live challenge
            // per user and purpose.
            var challenge = await dbContext.OtpChallenges
                .SingleOrDefaultAsync(
                    c => c.UserId == user.Id
                         && c.Purpose == OtpPurpose.Login
                         && c.ConsumedAtUtc == null
                         && c.InvalidatedAtUtc == null,
                    cancellationToken);

            if (challenge is null || !challenge.IsActive(utcNow, settings.MaxFailedAttempts))
            {
                // No writes here: dead-row cleanup belongs to RequestOtp's
                // invalidation, and an unauthenticated write on this path
                // would race the xmin token into avoidable 500s.
                DummyHash(command.Code);
                return AuthErrors.InvalidCode;
            }

            if (!otpCodeHasher.Verify(challenge.CodeHash, challenge.Id.Value, command.Code))
            {
                challenge.RegisterFailedAttempt();

                try
                {
                    // Persist the counter BEFORE returning — a crash must not
                    // forgive an attempt.
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // A parallel verify already advanced or killed this
                    // challenge; the generic 401 stands either way.
                    return AuthErrors.InvalidCode;
                }

                if (challenge.FailedAttempts >= settings.MaxFailedAttempts)
                {
                    logger.LogWarning(
                        "OtpLockedOut: challenge {ChallengeId} for user {UserId}",
                        challenge.Id.Value,
                        user.Id.Value);
                }

                return AuthErrors.InvalidCode;
            }

            challenge.Consume(utcNow);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Two concurrent verifies with the correct code: the loser of
                // the xmin race gets the generic 401 and no token.
                return AuthErrors.InvalidCode;
            }

            var accessToken = tokenService.CreateAccessToken(user);

            logger.LogInformation(
                "LoginSucceeded: challenge {ChallengeId} for user {UserId}",
                challenge.Id.Value,
                user.Id.Value);

            return new AuthResult(user, accessToken);
        }

        // Mirror the real path's challenge query and HMAC work for unknown
        // emails, so the cheap path is not obviously faster.
        private async Task EqualizeTimingAsync(string code, CancellationToken cancellationToken)
        {
            var throwawayUserId = UserId.Create(Guid.NewGuid());

            await dbContext.OtpChallenges
                .SingleOrDefaultAsync(
                    c => c.UserId == throwawayUserId
                         && c.Purpose == OtpPurpose.Login
                         && c.ConsumedAtUtc == null
                         && c.InvalidatedAtUtc == null,
                    cancellationToken);

            DummyHash(code);
        }

        private void DummyHash(string code) => otpCodeHasher.Hash(Guid.NewGuid(), code);
    }
}
