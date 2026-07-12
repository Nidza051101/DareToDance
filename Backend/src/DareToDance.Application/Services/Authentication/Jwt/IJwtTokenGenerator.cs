namespace DareToDance.Application.Services.Authentication.Jwt;

public interface IJwtTokenGenerator
{
    public string GenerateToken(TokenSubject subject);
}
