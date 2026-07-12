using DareToDance.Application.Services.Authentication.Jwt;

namespace DareToDance.Application.UnitTests.TestUtils;

public class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public TokenSubject? LastSubject { get; private set; }

    public string GenerateToken(TokenSubject subject)
    {
        LastSubject = subject;
        return $"token-for-{subject.Id}";
    }
}
