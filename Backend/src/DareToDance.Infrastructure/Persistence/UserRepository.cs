using DareToDance.Application.Common.Persistence;
using DareToDance.Domain.Entities;

namespace DareToDance.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly List<User> _users = [];

    public User? GetUserByEmail(string email)
    {
        return _users.SingleOrDefault(user => user.Email == email);
    }

    public void Add(User user)
    {
        _users.Add(user);
    }
}
