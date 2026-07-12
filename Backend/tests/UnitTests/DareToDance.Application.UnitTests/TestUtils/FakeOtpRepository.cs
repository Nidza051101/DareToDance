using DareToDance.Application.Common.Persistence;
using DareToDance.Domain.Entities;

namespace DareToDance.Application.UnitTests.TestUtils;

public class FakeOtpRepository : IOtpRepository
{
    public List<OtpCode> Codes { get; } = [];

    public void Add(OtpCode otpCode) => Codes.Add(otpCode);

    public OtpCode? GetLatestByUserId(Guid userId, OtpPurpose purpose) =>
        Codes
            .Where(code => code.UserId == userId && code.Purpose == purpose && !code.IsConsumed)
            .OrderByDescending(code => code.CreatedAt)
            .FirstOrDefault();

    public void Update(OtpCode otpCode)
    {
    }
}
