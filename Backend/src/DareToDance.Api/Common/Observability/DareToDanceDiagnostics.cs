using System.Diagnostics;

namespace DareToDance.Api.Common.Observability;

public static class DareToDanceDiagnostics
{
    public const string ActivitySourceName = "DareToDance";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
