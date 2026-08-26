using DareToDance.Api.Features.Auth.Shared;
using DareToDance.Infrastructure.Options;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DareToDance.Api.Features.Auth.Commands.GoogleLogin;

public static class GoogleLogin
{
    public sealed record Command(string IdToken) : IRequest<ErrorOr<AuthResult>>;

    public sealed class Handler(
        AppDbContext dbContext,
        ITokenService tokenService,
        IOptions<GoogleAuthSettings> googleOptions,
        IGoogleTokenValidator googleTokenValidator)
        : IRequestHandler<Command, ErrorOr<AuthResult>>
    {
        public async Task<ErrorOr<AuthResult>> Handle(
            Command command,
            CancellationToken cancellationToken)
        {
            var settings = googleOptions.Value;

            GoogleTokenPayload payload;

            try
            {
                payload = await googleTokenValidator.ValidateAsync(
                    command.IdToken,
                    settings.ClientId);
            }
            catch (Exception)
            {
                return Error.Validation("Token.Invalid", "Invalid Google token.");
            }

            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    u => u.Email == payload.Email,
                    cancellationToken);

            if (user is null)
            {
                return Error.NotFound(
                    "User.NotFound",
                    "Account does not exist.");
            }

            var utcNow = DateTime.UtcNow;

            var refreshTokenResult =
                tokenService.CreateRefreshToken(user.Id, utcNow);

            if (refreshTokenResult.IsError)
            {
                return refreshTokenResult.Errors;
            }

            var refreshToken = refreshTokenResult.Value;

            dbContext.RefreshTokens.Add(refreshToken.Token);

            await dbContext.SaveChangesAsync(cancellationToken);

            var accessToken = tokenService.CreateAccessToken(user);

            return new AuthResult(
                user,
                accessToken,
                refreshToken.WireToken,
                refreshToken.Token.ExpiresAtUtc);
        }
    }
}