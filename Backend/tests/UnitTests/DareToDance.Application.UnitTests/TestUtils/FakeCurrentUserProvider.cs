using DareToDance.Application.Common.Security;

namespace DareToDance.Application.UnitTests.TestUtils;

public class FakeCurrentUserProvider : ICurrentUserProvider
{
    public CurrentUser? CurrentUser { get; set; }

    public CurrentUser? GetCurrentUser() => CurrentUser;
}
