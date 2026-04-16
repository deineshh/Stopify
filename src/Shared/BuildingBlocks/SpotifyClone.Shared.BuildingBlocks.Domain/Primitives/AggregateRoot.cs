namespace SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;

public abstract class AggregateRoot<TId, TIdValue> : Entity<TId, TIdValue>, IHasDomainEvents
    where TId : notnull, StronglyTypedId<TIdValue>
    where TIdValue : notnull
{
    private readonly List<DomainEvent> _domainEvents = [];

    public DateTimeOffset CreatedAtUtc { get; init; }
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot()
        : base()
    {
    }

    protected AggregateRoot(TId id)
        : base(id)
        => CreatedAtUtc = DateTimeOffset.UtcNow;

    protected void RaiseDomainEvent(DomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();
}
