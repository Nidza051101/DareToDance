namespace DareToDance.Api.Features.Auth.Commands.VerifyLoginCode;

public static partial class VerifyLoginCode
{
    public static Command ToCommand(this VerifyLoginCodeRequest request)
    {
        return new Command(request.Recipient, request.Code);
    }

    public static AccessTokenResponse ToResponse(this Result result)
    {
        return new AccessTokenResponse(result.AccessToken, result.ExpiresAtUtc);
    }
}
