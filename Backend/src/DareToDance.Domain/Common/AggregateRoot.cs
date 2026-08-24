namespace DareToDance.Domain.Common;

public abstract class AggregateRoot<TId>(TId id, DateTime createdAtUtc, DateTime updatedAtUtc)
    : Entity<TId>(id)
    where TId : notnull
{
    public DateTime CreatedAtUtc { get; private set; } = createdAtUtc;
    public DateTime UpdatedAtUtc { get; private set; } = updatedAtUtc;

    protected AggregateRoot() : this(default!, default, default) { }
}
