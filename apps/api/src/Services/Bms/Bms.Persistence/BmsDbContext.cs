using BuildingBlocks.Contracts.Abstractions;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bms.Persistence;

/// <summary>
/// DbContext duy nhất của BMS (shared database, multi-tenant theo cột tenant_id).
/// - Tên bảng/cột snake_case (UseSnakeCaseNamingConvention, cấu hình trong DependencyInjection).
/// - Global filter "Tenant" + "SoftDelete" do <see cref="TenantDbContextBase"/> áp tự động
///   cho mọi entity ITenantEntity / ISoftDelete.
/// - Mỗi entity có 1 IEntityTypeConfiguration trong Configurations/{Module}/ — tự nạp bên dưới.
/// - DbSet: không bắt buộc; dùng IRepositoryBase&lt;TEntity, TKey&gt; hoặc Set&lt;TEntity&gt;().
/// </summary>
public sealed class BmsDbContext(
    DbContextOptions<BmsDbContext> options,
    ITenantContext tenantContext,
    ICompanyContext companyContext)
    : TenantDbContextBase(options, tenantContext, companyContext)
{
    protected override void ConfigureModel(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(BmsDbContext).Assembly);
}
