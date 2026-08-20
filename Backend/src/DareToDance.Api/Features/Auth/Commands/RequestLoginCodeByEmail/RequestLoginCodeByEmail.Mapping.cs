namespace DareToDance.Api.Features.Auth.Commands.RequestLoginCodeByEmail;

public static partial class RequestLoginCodeByEmail
{
    public static Command ToCommand(this RequestLoginCodeByEmailRequest request)
    {
        return new Command(request.Email);
    }
}
