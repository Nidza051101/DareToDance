using DareToDance.Application.Common.Persistence;
using DareToDance.Domain.Entities;

namespace DareToDance.Application.UnitTests.TestUtils;

public class FakeUserRepository : IUserRepository
{
    public List<User> Users { get; } = [];

    public User? GetUserByEmail(string email) => Users.SingleOrDefault(user => user.Email == email);

    public void Add(User user) => Users.Add(user);
}
