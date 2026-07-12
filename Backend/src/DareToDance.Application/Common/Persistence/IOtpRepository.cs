using DareToDance.Domain.Entities;

namespace DareToDance.Application.Common.Persistence;

public interface IOtpRepository
{
    void Add(OtpCode otpCode);

    /// <summary>Most recent non-consumed code for the user and purpose, or null.</summary>
    OtpCode? GetLatestByUserId(Guid userId, OtpPurpose purpose);

    void Update(OtpCode otpCode);
}
