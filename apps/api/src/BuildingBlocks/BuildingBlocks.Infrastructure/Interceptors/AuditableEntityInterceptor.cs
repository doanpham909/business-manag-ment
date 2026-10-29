using BuildingBlocks.Contracts.Abstractions;
using BuildingBlocks.Contracts.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Interceptors;

/// <summary>
/// Trước khi SaveChanges:
/// 1. Đổi Delete → Update is_deleted = true cho entity <see cref="ISoftDelete"/> (xóa mềm).
/// 2. Gán CreatedAt/By khi thêm, UpdatedAt/By khi sửa (UTC, từ <see cref="TimeProvider"/>).
/// Gộp UpdateAuditableEntitiesInterceptor + UpdateUserTrackingInterceptor của QMS.
/// </summary>
public sealed class AuditableEntityInterceptor(ICurrentUser currentUser, TimeProvider timeProvider)
    : SaveChangesInterceptor
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

        var now = timeProvider.GetUtcNow();
        var userId = currentUser.UserId;

        // Bước 1: xóa mềm — phải chạy trước để entry chuyển sang Modified rồi được stamp UpdatedAt
        // ToList(): không đổi State trong lúc đang duyệt change tracker
        var deletedEntries = context.ChangeTracker.Entries<ISoftDelete>()
            .Where(entry => entry.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in deletedEntries)
        {
            // Unchanged rồi chỉ đánh dấu cột xóa: tránh UPDATE toàn bộ cột bằng giá trị đang có trên entity
            // (entity "stub" chỉ có Id sẽ ghi đè dữ liệu thật bằng giá trị rỗng)
            entry.State = EntityState.Unchanged;
            entry.Property(nameof(ISoftDelete.IsDeleted)).CurrentValue = true;
            entry.Property(nameof(ISoftDelete.DeletedAt)).CurrentValue = now;
            entry.Property(nameof(ISoftDelete.DeletedBy)).CurrentValue = userId;
            entry.Property(nameof(ISoftDelete.IsDeleted)).IsModified = true;
            entry.Property(nameof(ISoftDelete.DeletedAt)).IsModified = true;
            entry.Property(nameof(ISoftDelete.DeletedBy)).IsModified = true;
        }

        // Bước 2: audit
        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(IAuditable.CreatedAt)).CurrentValue = now;
                    entry.Property(nameof(IAuditable.CreatedBy)).CurrentValue = userId;
                    break;

                case EntityState.Modified:
                    entry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = now;
                    entry.Property(nameof(IAuditable.UpdatedBy)).CurrentValue = userId;
                    // Không cho phép ghi đè thông tin tạo (kể cả khi entity bị Update() toàn bộ)
                    entry.Property(nameof(IAuditable.CreatedAt)).IsModified = false;
                    entry.Property(nameof(IAuditable.CreatedBy)).IsModified = false;
                    break;
            }
        }
    }
}
