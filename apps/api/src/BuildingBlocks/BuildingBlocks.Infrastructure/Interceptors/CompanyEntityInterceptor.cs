using BuildingBlocks.Contracts.Abstractions;
using BuildingBlocks.Contracts.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Interceptors;

/// <summary>
/// Cô lập dữ liệu giữa các công ty khi ghi (ADR-0002), cùng nguyên tắc với <see cref="TenantEntityInterceptor"/>:
/// - Insert: CompanyId rỗng → gán công ty đang làm việc; không có công ty cụ thể (chưa chọn, hoặc chế độ
///   "Tất cả công ty" chỉ đọc) → chặn. CompanyId set sẵn khác công ty đang làm việc → chặn.
///   Không có ngữ cảnh người dùng (seed / job) mà entity đã có CompanyId → cho phép.
/// - Update/Delete: bắt buộc có công ty đang làm việc, dòng phải thuộc công ty đó, không được đổi CompanyId.
/// </summary>
public sealed class CompanyEntityInterceptor(ICompanyContext companyContext, ITenantContext tenantContext)
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

        foreach (var entry in context.ChangeTracker.Entries<ICompanyEntity>())
        {
            var property = entry.Property(nameof(ICompanyEntity.CompanyId));

            if (entry.State == EntityState.Added)
                Stamp(entry, property);
            else if (entry.State is EntityState.Modified or EntityState.Deleted)
                EnsureOwnedByActiveCompany(entry, property);
        }
    }

    private void Stamp(EntityEntry<ICompanyEntity> entry, PropertyEntry property)
    {
        var name = entry.Metadata.ClrType.Name;
        var active = companyContext.ActiveCompanyId;
        var current = entry.Entity.CompanyId;

        if (current == Guid.Empty)
        {
            property.CurrentValue = active ?? throw new InvalidOperationException(
                $"Cannot insert {name}: no active company (read-only or not selected).");
            return;
        }

        // Ngữ cảnh người dùng (có tenant) bắt buộc ghi đúng công ty đang làm việc
        if (tenantContext.TenantId.HasValue && active != current)
            throw new InvalidOperationException($"Cannot insert {name} for another company.");
    }

    private void EnsureOwnedByActiveCompany(EntityEntry<ICompanyEntity> entry, PropertyEntry property)
    {
        var name = entry.Metadata.ClrType.Name;
        var active = companyContext.ActiveCompanyId ?? throw new InvalidOperationException(
            $"Cannot modify or delete {name}: no active company (read-only or not selected).");

        if (property.OriginalValue is not Guid original || original != active)
            throw new InvalidOperationException($"Cannot modify or delete {name} of another company.");

        if (!Equals(property.CurrentValue, original))
            throw new InvalidOperationException($"Changing CompanyId of {name} is not allowed.");
    }
}
