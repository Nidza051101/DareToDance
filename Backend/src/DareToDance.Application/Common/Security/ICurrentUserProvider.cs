namespace DareToDance.Application.Common.Security;

public interface ICurrentUserProvider
{
    CurrentUser? GetCurrentUser();
}
