namespace DareToDance.Domain.Common;

public interface IHasDomainEvent
{
    public IReadOnlyList<IDomainEvent> DomainEvents { get; }
    public void ClearDomainEvents();
}