namespace DareToDance.Api.Features.Auth.Commands.VerifyLoginCode;

public static class VerifyLoginCodeMapping
{
    public static VerifyLoginCodeCommand ToCommand(this VerifyLoginCodeRequest request)
    {
        return new VerifyLoginCodeCommand(request.Recipient, request.Code);
    }
}
