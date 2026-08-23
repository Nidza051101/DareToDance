namespace DareToDance.Infrastructure.Services;

public interface IOtpGenerator
{
    string Generate(int length);
}
