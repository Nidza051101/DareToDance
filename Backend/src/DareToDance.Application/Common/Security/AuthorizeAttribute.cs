namespace DareToDance.Application.Common.Security;

[AttributeUsage(AttributeTargets.Class)]
public class AuthorizeAttribute : Attribute
{
    public string? Roles { get; set; }
}
