using DareToDance.Api.Features.Auth.Shared;
using DareToDance.Domain.OtpChallenge;
using DareToDance.Domain.OtpChallenge.Id;
using DareToDance.Domain.User.Id;
using DareToDance.Infrastructure.Options;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DareToDance.Api.Features.Auth.Commands.RequestOtp;

public static class RequestOtp
{
    public sealed record Command(string Email) : IRequest<ErrorOr<Success>>;

    // Every outcome returns Success: unknown email, resend cooldown, and the
    // daily cap are handled silently, because any distinct response on an
    // unauthenticated endpoint would reveal whether an account exists.
    public sealed class Handler(
        AppDbContext dbContext,
        IOtpGenerator otpGenerator,
        IOtpCodeHasher otpCodeHasher,
        IOtpSender otpSender,
        IOptions<OtpSettings> otpOptions,
        TimeProvider timeProvider,
        ILogger<Handler> logger)
        : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var settings = otpOptions.Value;
            var utcNow = timeProvider.GetUtcNow().UtcDateTime;
            var email = AuthEmail.Normalize(command.Email);

            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user is null)
            {
                await EqualizeTimingAsync(utcNow, cancellationToken);
                return Result.Success;
            }

            var windowStartUtc = utcNow.AddHours(-24);

            var recentCreations = await dbContext.OtpChallenges
                .Where(c => c.UserId == user.Id
                            && c.Purpose == OtpPurpose.Login
                            && c.CreatedAtUtc > windowStartUtc)
                .Select(c => c.CreatedAtUtc)
                .ToListAsync(cancellationToken);

            if (recentCreations.Count >= settings.MaxCodesPerDay)
            {
                logger.LogWarning("OtpDailyCapHit for user {UserId}", user.Id.Value);
                return Result.Success;
            }

            // Cooldown counts the newest challenge of ANY state — measuring
            // only live ones would let a caller burn all attempts and
            // immediately request a fresh code.
            var cooldownStartUtc = utcNow.AddSeconds(-settings.ResendCooldownSeconds);

            if (recentCreations.Count > 0 && recentCreations.Max() > cooldownStartUtc)
            {
                return Result.Success;
            }

            var challengeId = OtpChallengeId.CreateUnique();
            var code = otpGenerator.Generate(settings.CodeLength);
            var codeHash = otpCodeHasher.Hash(challengeId.Value, code);

            var challenge = OtpChallenge.Create(
                challengeId,
                user.Id,
                codeHash,
                OtpPurpose.Login,
                utcNow,
                TimeSpan.FromSeconds(settings.ExpirySeconds));

            try
            {
                await using var transaction =
                    await dbContext.Database.BeginTransactionAsync(cancellationToken);

                // The predicate must match ix_otp_challenges_user_id_purpose_active's
                // filter exactly — expired rows included — or a dead row would keep
                // the unique slot occupied and silently block new codes forever.
                await dbContext.OtpChallenges
                    .Where(c => c.UserId == user.Id
                                && c.Purpose == OtpPurpose.Login
                                && c.ConsumedAtUtc == null
                                && c.InvalidatedAtUtc == null)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(c => c.InvalidatedAtUtc, utcNow),
                        cancellationToken);

                dbContext.OtpChallenges.Add(challenge);
                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (
                ex.InnerException is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation,
                    ConstraintName: "ix_otp_challenges_user_id_purpose_active"
                })
            {
                // A concurrent request won the race; its code is on the way.
                logger.LogInformation("OtpChallengeInsertRace for user {UserId}", user.Id.Value);
                return Result.Success;
            }

            // Send only after a successful commit — a rolled-back challenge
            // must never produce a delivered code.
            await otpSender.SendAsync(
                new OtpNotification(user.Email, code, challenge.ExpiresAtUtc),
                cancellationToken);

            logger.LogInformation(
                "OtpRequested: challenge {ChallengeId} for user {UserId}",
                challengeId.Value,
                user.Id.Value);

            return Result.Success;
        }

        // Best-effort equalization for unknown emails: run the same guard query
        // and generate+hash work the real path performs. The remaining
        // insert/send delta is accepted and mitigated by the constant 202
        // response and rate limiting.
        private async Task EqualizeTimingAsync(DateTime utcNow, CancellationToken cancellationToken)
        {
            var settings = otpOptions.Value;
            var throwawayUserId = UserId.Create(Guid.NewGuid());
            var windowStartUtc = utcNow.AddHours(-24);

            await dbContext.OtpChallenges
                .Where(c => c.UserId == throwawayUserId
                            && c.Purpose == OtpPurpose.Login
                            && c.CreatedAtUtc > windowStartUtc)
                .Select(c => c.CreatedAtUtc)
                .ToListAsync(cancellationToken);

            var code = otpGenerator.Generate(settings.CodeLength);
            otpCodeHasher.Hash(Guid.NewGuid(), code);
        }
    }
}
