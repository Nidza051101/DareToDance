using DareToDance.Application.Common.Persistence;
using DareToDance.Domain.Entities;

namespace DareToDance.Infrastructure.Persistence;

public class OtpRepository : IOtpRepository
{
    private readonly List<OtpCode> _codes = [];

    public void Add(OtpCode otpCode)
    {
        _codes.Add(otpCode);
    }

    public OtpCode? GetLatestByUserId(Guid userId, OtpPurpose purpose)
    {
        return _codes
            .Where(code => code.UserId == userId && code.Purpose == purpose && !code.IsConsumed)
            .OrderByDescending(code => code.CreatedAt)
            .FirstOrDefault();
    }

    public void Update(OtpCode otpCode)
    {
        // in-memory list holds live references; nothing to do until EF Core replaces this
    }
}
