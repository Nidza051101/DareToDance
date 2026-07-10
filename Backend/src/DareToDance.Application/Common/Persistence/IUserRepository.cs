using DareToDance.Domain.Entities;

namespace DareToDance.Application.Common.Persistence;

public interface IUserRepository
{
    User? GetUserByEmail(string email);
    void Add(User user);
}
