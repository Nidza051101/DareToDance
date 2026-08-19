namespace DareToDance.Api.Features.Auth.Commands.RequestLoginCodeByEmail;

public static class RequestLoginCodeByEmailMapping
{
    public static RequestLoginCodeByEmailCommand ToCommand(this RequestLoginCodeByEmailRequest request)
    {
        return new RequestLoginCodeByEmailCommand(request.Email);
    }
}
