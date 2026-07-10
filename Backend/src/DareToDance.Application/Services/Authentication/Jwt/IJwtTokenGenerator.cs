namespace DareToDance.Application.Services.Authentication.Jwt;

public interface IJwtTokenGenerator
{
    public string GenerateToken(Guid userId, string firstName, string lastName);
}