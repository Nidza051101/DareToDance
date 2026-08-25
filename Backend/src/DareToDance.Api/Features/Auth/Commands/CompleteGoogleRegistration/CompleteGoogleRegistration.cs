using DareToDance.Api.Features.Auth.Shared;
using DareToDance.Api.Features.Users.Shared;
using DareToDance.Domain.User;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DareToDance.Api.Features.Auth.Commands.CompleteGoogleRegistration;

public static class CompleteGoogleRegistration
{
    public sealed record Command(string IdToken, string Phone) : IRequest<ErrorOr<AuthResult>>
    {
        public override string ToString()
            => "CompleteGoogleRegistration.Command { IdToken = [REDACTED], Phone = [REDACTED] }";
    }

    public sealed class Handler(
        AppDbContext dbContext,
        IGoogleTokenVerifier googleTokenVerifier,
        ITokenService tokenService,
        TimeProvider timeProvider,
        ILogger<Handler> logger)
        : IRequestHandler<Command, ErrorOr<AuthResult>>
    {
        public async Task<ErrorOr<AuthResult>> Handle(Command command, CancellationToken cancellationToken)
        {
            var identityResult = await googleTokenVerifier.VerifyAsync(command.IdToken, cancellationToken);

            if (identityResult.IsError)
            {
                return identityResult.Errors;
            }

            var identity = identityResult.Value;
            var email = AuthEmail.Normalize(identity.Email);
            var phone = command.Phone.Trim();

            // Pre-checks give a clean, specific error on the common path; the
            // unique-index catch below still guards the rare concurrent race.
            var emailTaken = await dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email == email, cancellationToken);

            if (emailTaken)
            {
                return UserErrors.DuplicateEmail;
            }

            var phoneTaken = await dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.Phone == phone, cancellationToken);

            if (phoneTaken)
            {
                return UserErrors.DuplicatePhone;
            }

            var user = User.Create(identity.FirstName, identity.LastName, email, phone);
            dbContext.Users.Add(user);

            var utcNow = timeProvider.GetUtcNow().UtcDateTime;

            var refreshTokenResult = tokenService.CreateRefreshToken(user.Id, utcNow);

            if (refreshTokenResult.IsError)
            {
                return refreshTokenResult.Errors;
            }

            var refreshToken = refreshTokenResult.Value;
            dbContext.RefreshTokens.Add(refreshToken.Token);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            } pg)
            {
                // Someone else registered with the same email or phone between our
                // check and this insert. Generic error either way - no need to
                // reveal which one, and the caller can just retry.
                logger.LogInformation(
                    "GoogleRegistrationRace on constraint {Constraint}",
                    pg.ConstraintName);

                return pg.ConstraintName == "ix_users_phone"
                    ? UserErrors.DuplicatePhone
                    : UserErrors.DuplicateEmail;
            }

            var accessToken = tokenService.CreateAccessToken(user);

            logger.LogInformation("GoogleRegistrationCompleted: user {UserId}", user.Id.Value);

            return new AuthResult(user, accessToken, refreshToken.WireToken, refreshToken.Token.ExpiresAtUtc);
        }
    }
}
