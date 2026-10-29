using MediatR;

namespace BuildingBlocks.Contracts.Domain.DDD;

/// <summary>Domain event (MediatR notification). Đặt tên thì quá khứ: {Domain}{Action}Event.</summary>
public interface IDomainEvent : INotification
{
    Guid Id { get; }

    DateTimeOffset OccurredOn { get; }
}

/// <summary>Base record cho domain event.</summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>Entity giữ danh sách domain event chờ publish.</summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}

/// <summary>Marker interface cho Aggregate Root (entry point duy nhất của aggregate).</summary>
public interface IAggregateRoot;
