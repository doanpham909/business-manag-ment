using BuildingBlocks.Contracts.Domain.DDD;

namespace BuildingBlocks.Contracts.Domain.Entities;

/// <summary>
/// Entity có audit + domain events — base mặc định cho Aggregate Root
/// (kết hợp với <see cref="IAggregateRoot"/>). Events được publish sau khi SaveChanges thành công.
/// </summary>
public abstract class EventEntityAuditBase<TKey> : EntityAuditBase<TKey>, IHasDomainEvents
    where TKey : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
