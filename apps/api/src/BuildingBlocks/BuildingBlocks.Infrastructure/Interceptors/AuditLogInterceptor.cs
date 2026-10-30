using System.Reflection;
using System.Text.Json;
using BuildingBlocks.Contracts.Abstractions;
using BuildingBlocks.Contracts.Domain.Entities;
using BuildingBlocks.Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Interceptors;

/// <summary>
/// Ghi <see cref="AuditLog"/> cho mọi entity được thêm/sửa/xóa trong lần SaveChanges.
/// Chạy SAU interceptor tenant/công ty (đã gán TenantId/CompanyId) và TRƯỚC interceptor audit
/// (để thấy trạng thái Deleted trước khi bị đổi thành xóa mềm). Thuộc tính [Sensitive] hoặc tên có
/// "Password"/"Hash" chỉ ghi "***". Bỏ qua entity không xác định được tenant (vd. token đăng nhập).
/// </summary>
public sealed class AuditLogInterceptor(
    ITenantContext tenantContext,
    ICompanyContext companyContext,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : SaveChangesInterceptor
{
    private const string Masked = "***";

    private static readonly string[] SkippedFields =
    [
        nameof(IAuditable.CreatedAt), nameof(IAuditable.CreatedBy),
        nameof(IAuditable.UpdatedAt), nameof(IAuditable.UpdatedBy),
    ];

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
        var logs = context.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog and not INotAudited
                        && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => ToLog(e, now))
            .OfType<AuditLog>()
            .ToList();

        if (logs.Count > 0)
            context.Set<AuditLog>().AddRange(logs);
    }

    private AuditLog? ToLog(EntityEntry entry, DateTimeOffset now)
    {
        // Entity con (không ITenantEntity) chỉ được ghi khi có tenant của người dùng hiện tại
        var tenantId = entry.Entity is ITenantEntity tenantEntity ? tenantEntity.TenantId : tenantContext.TenantId;
        if (tenantId is null || tenantId == Guid.Empty)
            return null;

        var changes = new Dictionary<string, object?>();
        foreach (var property in entry.Properties)
        {
            var name = property.Metadata.Name;
            if (SkippedFields.Contains(name) || (entry.State == EntityState.Modified && !property.IsModified))
                continue;

            var sensitive = IsSensitive(property);
            changes[name] = entry.State switch
            {
                EntityState.Added => new { @new = Mask(property.CurrentValue, sensitive) },
                EntityState.Deleted => new { old = Mask(property.OriginalValue, sensitive) },
                _ => new { old = Mask(property.OriginalValue, sensitive), @new = Mask(property.CurrentValue, sensitive) },
            };
        }

        var action = entry.State switch
        {
            EntityState.Added => AuditAction.Created,
            EntityState.Deleted => AuditAction.Deleted,
            _ => IsSoftDeleteMarked(entry) ? AuditAction.Deleted : AuditAction.Updated,
        };

        var keyProperties = entry.Metadata.FindPrimaryKey()?.Properties ?? [];
        var key = string.Join(",", keyProperties.Select(p => entry.Property(p.Name).CurrentValue?.ToString()));
        var companyId = entry.Entity is ICompanyEntity companyEntity
            ? companyEntity.CompanyId
            : companyContext.ActiveCompanyId;

        return AuditLog.Create(tenantId.Value, companyId, currentUser.UserId, entry.Metadata.ClrType.Name,
            key, action, JsonSerializer.Serialize(changes), now);
    }

    private static object? Mask(object? value, bool sensitive) => sensitive && value is not null ? Masked : value;

    private static bool IsSoftDeleteMarked(EntityEntry entry) =>
        entry.Entity is ISoftDelete
        && entry.Property(nameof(ISoftDelete.IsDeleted)) is { IsModified: true, CurrentValue: true };

    private static bool IsSensitive(PropertyEntry property) =>
        property.Metadata.Name.Contains("Password", StringComparison.OrdinalIgnoreCase)
        || property.Metadata.Name.Contains("Hash", StringComparison.OrdinalIgnoreCase)
        || property.Metadata.PropertyInfo?.GetCustomAttribute<SensitiveAttribute>() is not null;
}
