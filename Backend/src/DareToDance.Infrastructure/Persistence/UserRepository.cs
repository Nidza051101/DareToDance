using DareToDance.Application.Common.Persistence;
using DareToDance.Domain.Entities;

namespace DareToDance.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private static readonly List<User> Users = [];

    public User? GetUserByEmail(string email)
    {
        return Users.SingleOrDefault(user => user.Email == email);
    }

    public void Add(User user)
    {
        Users.Add(user);
    }
}
