using BuildingBlocks.Contracts.Abstractions;
using BuildingBlocks.Contracts.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Interceptors;

/// <summary>
/// Bảo vệ cô lập dữ liệu giữa các tenant khi ghi:
/// - Insert: TenantId rỗng → gán từ <see cref="ITenantContext"/>; không có tenant → chặn.
///   TenantId được set sẵn khác tenant hiện tại → chặn (ghi chéo tenant).
///   Không có tenant context (job hệ thống/seed) mà entity đã có TenantId → cho phép.
/// - Update/Delete: bắt buộc có tenant context, dòng phải thuộc tenant hiện tại (theo giá trị gốc)
///   và không được đổi TenantId. Lớp phòng thủ thứ hai — lớp chính là RepositoryBase chỉ cho
///   Update/Remove entity đã được load (qua query có tenant filter).
/// Vi phạm ném <see cref="InvalidOperationException"/> → HTTP 500 (lỗi lập trình, không phải lỗi người dùng).
/// </summary>
public sealed class TenantEntityInterceptor(ITenantContext tenantContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext? context)
    {
        if (context is null)
            return;

        foreach (var entry in context.ChangeTracker.Entries<ITenantEntity>())
        {
            var tenantProperty = entry.Property(nameof(ITenantEntity.TenantId));

            if (entry.State == EntityState.Added)
                StampTenant(entry, tenantProperty);
            else if (entry.State is EntityState.Modified or EntityState.Deleted)
                EnsureOwnedByCurrentTenant(entry, tenantProperty);
        }
    }

    private void EnsureOwnedByCurrentTenant(EntityEntry<ITenantEntity> entry, PropertyEntry tenantProperty)
    {
        var entityName = entry.Metadata.ClrType.Name;
        var currentTenantId = tenantContext.TenantId ?? throw new InvalidOperationException(
            $"Cannot modify or delete {entityName}: no tenant in the current context.");

        if (tenantProperty.OriginalValue is not Guid originalTenantId || originalTenantId != currentTenantId)
            throw new InvalidOperationException($"Cannot modify or delete {entityName} of another tenant.");

        if (!Equals(tenantProperty.CurrentValue, originalTenantId))
            throw new InvalidOperationException($"Changing TenantId of {entityName} is not allowed.");
    }

    private void StampTenant(EntityEntry<ITenantEntity> entry, PropertyEntry tenantProperty)
    {
        var currentTenantId = tenantContext.TenantId;
        var entityTenantId = entry.Entity.TenantId;

        if (entityTenantId == Guid.Empty)
        {
            tenantProperty.CurrentValue = currentTenantId ?? throw new InvalidOperationException(
                $"Cannot insert {entry.Metadata.ClrType.Name}: no tenant in the current context.");
            return;
        }

        if (currentTenantId.HasValue && currentTenantId.Value != entityTenantId)
            throw new InvalidOperationException(
                $"Cannot insert {entry.Metadata.ClrType.Name} for another tenant.");
    }
}
