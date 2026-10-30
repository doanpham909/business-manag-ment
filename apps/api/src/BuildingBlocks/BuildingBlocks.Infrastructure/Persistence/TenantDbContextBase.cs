using System.Reflection;
using BuildingBlocks.Contracts.Abstractions;
using BuildingBlocks.Contracts.Domain.Entities;
using BuildingBlocks.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// DbContext base cho SaaS multi-tenant (shared database, cột tenant_id):
/// - Global query filter có tên (EF Core 10): "Tenant" (TenantId == tenant hiện tại) và
///   "SoftDelete" (!IsDeleted). Hai filter độc lập nên có thể tắt riêng từng cái —
///   xem <see cref="QueryFilterNames"/> để biết cách bypass cho thao tác hệ thống/admin.
/// - Không có tenant trong context → filter so sánh với NULL → không trả bản ghi nào (fail-closed).
/// - Khóa Guid: ValueGeneratedNever (Id do domain sinh bằng Guid.CreateVersion7()).
/// Lớp con cấu hình model trong <see cref="ConfigureModel"/>.
/// </summary>
public abstract class TenantDbContextBase(
    DbContextOptions options,
    ITenantContext tenantContext,
    ICompanyContext companyContext)
    : DbContext(options)
{
    private static readonly MethodInfo ApplyFiltersMethod = typeof(TenantDbContextBase)
        .GetMethod(nameof(ApplyFilters), BindingFlags.Instance | BindingFlags.NonPublic)!;

    /// <summary>
    /// Tenant hiện tại. Filter tham chiếu property của DbContext nên EF tham số hóa
    /// giá trị theo từng instance (mỗi request một tenant) — KHÔNG được thay bằng hằng số.
    /// </summary>
    public Guid? CurrentTenantId => tenantContext.TenantId;

    /// <summary>
    /// Các công ty được đọc (ADR-0002). Mảng rỗng khi chưa chọn công ty → entity ICompanyEntity
    /// không trả bản ghi nào (fail-closed). Cũng được EF tham số hóa theo từng instance.
    /// </summary>
    public Guid[] CurrentReadableCompanyIds => [.. companyContext.ReadableCompanyIds];

    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureModel(modelBuilder);
        ApplyConventions(modelBuilder);
    }

    /// <summary>Cấu hình entity (thường là ApplyConfigurationsFromAssembly).</summary>
    protected abstract void ConfigureModel(ModelBuilder modelBuilder);

    private void ApplyConventions(ModelBuilder modelBuilder)
    {
        // ToList(): tránh sửa model trong lúc duyệt
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList())
        {
            // Filter chỉ khai báo trên entity gốc của cây kế thừa; bỏ qua owned type
            if (entityType.BaseType is not null || entityType.IsOwned())
                continue;

            UseClientGeneratedGuidKeys(entityType);

            // Phiên bản chống sửa đè (CMN-10): uint + IsRowVersion → Npgsql ánh xạ cột hệ thống xmin
            if (typeof(IVersioned).IsAssignableFrom(entityType.ClrType))
                modelBuilder.Entity(entityType.ClrType).Property(nameof(IVersioned.Version)).IsRowVersion();
            ApplyFiltersMethod.MakeGenericMethod(entityType.ClrType).Invoke(this, [modelBuilder]);
        }
    }

    private static void UseClientGeneratedGuidKeys(IMutableEntityType entityType)
    {
        var keyProperties = entityType.FindPrimaryKey()?.Properties ?? [];
        foreach (var property in keyProperties.Where(p => p.ClrType == typeof(Guid)))
            property.ValueGenerated = ValueGenerated.Never;
    }

    private void ApplyFilters<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class
    {
        var builder = modelBuilder.Entity<TEntity>();

        if (typeof(ITenantEntity).IsAssignableFrom(typeof(TEntity)))
        {
            builder.HasQueryFilter(QueryFilterNames.Tenant,
                entity => ((ITenantEntity)entity).TenantId == CurrentTenantId);
            builder.HasIndex(nameof(ITenantEntity.TenantId));
        }

        if (typeof(ICompanyEntity).IsAssignableFrom(typeof(TEntity)))
        {
            builder.HasQueryFilter(QueryFilterNames.Company,
                entity => CurrentReadableCompanyIds.Contains(((ICompanyEntity)entity).CompanyId));
            builder.HasIndex(nameof(ITenantEntity.TenantId), nameof(ICompanyEntity.CompanyId));
        }

        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            builder.HasQueryFilter(QueryFilterNames.SoftDelete,
                entity => !((ISoftDelete)entity).IsDeleted);
        }
    }
}
