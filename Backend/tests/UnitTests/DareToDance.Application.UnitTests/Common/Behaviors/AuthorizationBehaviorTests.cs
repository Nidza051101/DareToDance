using DareToDance.Application.Common.Behaviors;
using DareToDance.Application.Common.Security;
using DareToDance.Application.UnitTests.TestUtils;
using ErrorOr;
using MediatR;

namespace DareToDance.Application.UnitTests.Common.Behaviors;

public record OpenRequest : IRequest<ErrorOr<Success>>;

[Authorize]
public record ProtectedRequest : IRequest<ErrorOr<Success>>;

[Authorize(Roles = "Admin")]
public record AdminOnlyRequest : IRequest<ErrorOr<Success>>;

[Authorize(Roles = "Admin, Manager")]
public record MultiRoleRequest : IRequest<ErrorOr<Success>>;

public class AuthorizationBehaviorTests
{
    private readonly FakeCurrentUserProvider _currentUserProvider = new();
    private bool _nextCalled;

    private Task<ErrorOr<Success>> Next(CancellationToken cancellationToken = default)
    {
        _nextCalled = true;
        return Task.FromResult<ErrorOr<Success>>(Result.Success);
    }

    private static CurrentUser UserWithRoles(params string[] roles) =>
        new(Guid.NewGuid(), "dancer@test.com", "Test", "Dancer", roles);

    [Fact]
    public async Task Handle_UnmarkedRequest_PassesThroughEvenWhenAnonymous()
    {
        var behavior = new AuthorizationBehavior<OpenRequest, ErrorOr<Success>>(_currentUserProvider);

        var result = await behavior.Handle(new OpenRequest(), Next, default);

        Assert.True(_nextCalled);
        Assert.False(result.IsError);
    }

    [Fact]
    public async Task Handle_ProtectedRequest_AnonymousCaller_ReturnsUnauthorizedWithoutInvokingHandler()
    {
        var behavior = new AuthorizationBehavior<ProtectedRequest, ErrorOr<Success>>(_currentUserProvider);

        var result = await behavior.Handle(new ProtectedRequest(), Next, default);

        Assert.False(_nextCalled);
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Unauthorized, result.FirstError.Type);
    }

    [Fact]
    public async Task Handle_ProtectedRequest_AuthenticatedCaller_PassesThrough()
    {
        _currentUserProvider.CurrentUser = UserWithRoles();
        var behavior = new AuthorizationBehavior<ProtectedRequest, ErrorOr<Success>>(_currentUserProvider);

        var result = await behavior.Handle(new ProtectedRequest(), Next, default);

        Assert.True(_nextCalled);
        Assert.False(result.IsError);
    }

    [Fact]
    public async Task Handle_RoleRequest_CallerWithoutRole_ReturnsForbidden()
    {
        _currentUserProvider.CurrentUser = UserWithRoles();
        var behavior = new AuthorizationBehavior<AdminOnlyRequest, ErrorOr<Success>>(_currentUserProvider);

        var result = await behavior.Handle(new AdminOnlyRequest(), Next, default);

        Assert.False(_nextCalled);
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Forbidden, result.FirstError.Type);
    }

    [Fact]
    public async Task Handle_RoleRequest_CallerWithRole_PassesThrough()
    {
        _currentUserProvider.CurrentUser = UserWithRoles("Admin");
        var behavior = new AuthorizationBehavior<AdminOnlyRequest, ErrorOr<Success>>(_currentUserProvider);

        var result = await behavior.Handle(new AdminOnlyRequest(), Next, default);

        Assert.True(_nextCalled);
        Assert.False(result.IsError);
    }

    [Fact]
    public async Task Handle_MultiRoleRequest_RequiresEveryListedRole()
    {
        _currentUserProvider.CurrentUser = UserWithRoles("Admin");
        var behavior = new AuthorizationBehavior<MultiRoleRequest, ErrorOr<Success>>(_currentUserProvider);

        var partial = await behavior.Handle(new MultiRoleRequest(), Next, default);
        Assert.True(partial.IsError);
        Assert.Equal(ErrorType.Forbidden, partial.FirstError.Type);

        _currentUserProvider.CurrentUser = UserWithRoles("Admin", "Manager");
        var full = await behavior.Handle(new MultiRoleRequest(), Next, default);

        Assert.False(full.IsError);
        Assert.True(_nextCalled);
    }
}
