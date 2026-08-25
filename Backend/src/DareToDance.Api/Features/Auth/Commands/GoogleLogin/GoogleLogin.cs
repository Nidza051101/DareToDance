using DareToDance.Api.Features.Auth.Shared;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DareToDance.Api.Features.Auth.Commands.GoogleLogin;

public static class GoogleLogin
{
    public sealed record Command(string IdToken) : IRequest<ErrorOr<GoogleLoginOutcome>>
    {
        public override string ToString() => "GoogleLogin.Command { IdToken = [REDACTED] }";
    }

    // Two equally valid, expected outcomes - not a success/error split. An
    // unknown email is not a failure, it's a signal to go complete registration.
    public abstract record GoogleLoginOutcome;

    public sealed record LoggedIn(AuthResult Result) : GoogleLoginOutcome;

    public sealed record AccountNotFound(string Email, string FirstName, string LastName) : GoogleLoginOutcome;

    public sealed class Handler(
        AppDbContext dbContext,
        IGoogleTokenVerifier googleTokenVerifier,
        ITokenService tokenService,
        TimeProvider timeProvider,
        ILogger<Handler> logger)
        : IRequestHandler<Command, ErrorOr<GoogleLoginOutcome>>
    {
        public async Task<ErrorOr<GoogleLoginOutcome>> Handle(Command command, CancellationToken cancellationToken)
        {
            var identityResult = await googleTokenVerifier.VerifyAsync(command.IdToken, cancellationToken);

            if (identityResult.IsError)
            {
                return identityResult.Errors;
            }

            var identity = identityResult.Value;
            var email = AuthEmail.Normalize(identity.Email);

            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user is null)
            {
                return new AccountNotFound(email, identity.FirstName, identity.LastName);
            }

            var utcNow = timeProvider.GetUtcNow().UtcDateTime;

            var refreshTokenResult = tokenService.CreateRefreshToken(user.Id, utcNow);

            if (refreshTokenResult.IsError)
            {
                return refreshTokenResult.Errors;
            }

            var refreshToken = refreshTokenResult.Value;
            dbContext.RefreshTokens.Add(refreshToken.Token);
            await dbContext.SaveChangesAsync(cancellationToken);

            var accessToken = tokenService.CreateAccessToken(user);

            logger.LogInformation("GoogleLoginSucceeded: user {UserId}", user.Id.Value);

            return new LoggedIn(new AuthResult(user, accessToken, refreshToken.WireToken, refreshToken.Token.ExpiresAtUtc));
        }
    }
}
