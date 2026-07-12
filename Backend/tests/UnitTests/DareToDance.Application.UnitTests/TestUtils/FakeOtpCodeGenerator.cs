using DareToDance.Application.Services.Authentication.Otp;

namespace DareToDance.Application.UnitTests.TestUtils;

/// <summary>Deterministic generator: sequential codes, transparent "hash" for easy assertions.</summary>
public class FakeOtpCodeGenerator : IOtpCodeGenerator
{
    private int _next = 100000;

    public string? LastIssuedCode { get; private set; }

    public GeneratedOtp Generate()
    {
        LastIssuedCode = _next++.ToString();
        return new GeneratedOtp(LastIssuedCode, HashOf(LastIssuedCode));
    }

    public bool Matches(string code, string codeHash) => HashOf(code) == codeHash;

    public static string HashOf(string code) => $"hash::{code}";
}
