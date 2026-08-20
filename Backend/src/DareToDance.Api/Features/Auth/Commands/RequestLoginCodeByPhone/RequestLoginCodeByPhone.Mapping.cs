namespace DareToDance.Api.Features.Auth.Commands.RequestLoginCodeByPhone;

public static partial class RequestLoginCodeByPhone
{
    public static Command ToCommand(this RequestLoginCodeByPhoneRequest request)
    {
        return new Command(request.Phone);
    }
}
