using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DareToDance.Infrastructure.Persistence.Interceptors;

// Stamps UpdatedAtUtc on every modified aggregate root so domain code
// doesn't have to remember to do it on each mutation.
public sealed class UpdateTimestampInterceptor : SaveChangesInterceptor
{
    private const string UpdatedAtUtc = "UpdatedAtUtc";

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        StampUpdatedAt(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        StampUpdatedAt(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void StampUpdatedAt(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var utcNow = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Modified &&
                entry.Metadata.FindProperty(UpdatedAtUtc) is not null)
            {
                entry.Property(UpdatedAtUtc).CurrentValue = utcNow;
            }
        }
    }
}
