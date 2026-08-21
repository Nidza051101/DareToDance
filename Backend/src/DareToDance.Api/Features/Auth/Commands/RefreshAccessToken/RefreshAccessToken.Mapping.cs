using DareToDance.Api.Features.Auth.Shared;

namespace DareToDance.Api.Features.Auth.Commands.RefreshAccessToken;

public static partial class RefreshAccessToken
{
    public static Command ToCommand(this RefreshAccessTokenRequest request)
    {
        return new Command(request.RefreshToken);
    }

    public static AuthTokensResponse ToResponse(this Result result)
    {
        return new AuthTokensResponse(
            result.AccessToken,
            result.AccessTokenExpiresAtUtc,
            result.RefreshToken,
            result.RefreshTokenExpiresAtUtc);
    }
}
