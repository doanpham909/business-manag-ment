using BuildingBlocks.Contracts.Domain.DDD;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Interceptors;

/// <summary>
/// Sau khi SaveChanges THÀNH CÔNG: lấy domain event từ các aggregate đang track, xóa khỏi entity
/// rồi publish qua MediatR (handler = <c>IDomainEventHandler&lt;TEvent&gt;</c>).
/// Lưu ý: handler chạy sau khi dữ liệu đã commit; cần đảm bảo tin cậy tuyệt đối → dùng Outbox (chưa làm, YAGNI).
/// </summary>
public sealed class PublishDomainEventsInterceptor(IPublisher publisher) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            await PublishAsync(eventData.Context, cancellationToken);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task PublishAsync(DbContext context, CancellationToken cancellationToken)
    {
        var events = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .SelectMany(entity =>
            {
                var pending = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();
                return pending;
            })
            .ToList();

        foreach (var domainEvent in events)
            await publisher.Publish(domainEvent, cancellationToken);
    }
}
