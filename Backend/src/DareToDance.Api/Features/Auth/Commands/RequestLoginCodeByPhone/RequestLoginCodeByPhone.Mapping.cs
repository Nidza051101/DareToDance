namespace DareToDance.Api.Features.Auth.Commands.RequestLoginCodeByPhone;

public static class RequestLoginCodeByPhoneMapping
{
    public static RequestLoginCodeByPhoneCommand ToCommand(this RequestLoginCodeByPhoneRequest request)
    {
        return new RequestLoginCodeByPhoneCommand(request.Phone);
    }
}
