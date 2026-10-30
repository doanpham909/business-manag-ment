using Bms.Domain.Admin.Catalogs;
using Bms.Domain.Admin.Companies;
using Bms.Domain.Admin.Roles;
using Bms.Domain.Admin.Tenants;
using Bms.Domain.Admin.Users;
using Bms.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bms.Persistence.Configurations.Admin;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable(TableNames.Tenants, TableNames.AdminSchema);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique().HasFilter("is_deleted = false");
    }
}

internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable(TableNames.Companies, TableNames.AdminSchema);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.TaxCode).HasMaxLength(20);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("is_deleted = false");
    }
}

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(TableNames.Users, TableNames.AdminSchema);
        builder.Property(x => x.Username).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);

        // Tên đăng nhập duy nhất toàn nền tảng (ADR-0002)
        builder.HasIndex(x => x.Username).IsUnique().HasFilter("is_deleted = false");
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId });

        builder.HasMany(x => x.CompanyAccesses).WithOne().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.RoleAssignments).WithOne().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.CompanyAccesses).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.RoleAssignments).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class UserCompanyAccessConfiguration : IEntityTypeConfiguration<UserCompanyAccess>
{
    public void Configure(EntityTypeBuilder<UserCompanyAccess> builder)
    {
        builder.ToTable(TableNames.UserCompanyAccesses, TableNames.AdminSchema);
        builder.HasIndex(x => new { x.UserId, x.CompanyId }).IsUnique();
    }
}

internal sealed class UserRoleAssignmentConfiguration : IEntityTypeConfiguration<UserRoleAssignment>
{
    public void Configure(EntityTypeBuilder<UserRoleAssignment> builder)
    {
        builder.ToTable(TableNames.UserRoleAssignments, TableNames.AdminSchema);
        builder.HasIndex(x => new { x.UserId, x.CompanyId, x.RoleId }).IsUnique();
        builder.HasIndex(x => x.RoleId);
    }
}

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable(TableNames.Roles, TableNames.AdminSchema);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("is_deleted = false");

        builder.HasMany(x => x.Permissions).WithOne().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.DataScopes).WithOne().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Permissions).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.DataScopes).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable(TableNames.RolePermissions, TableNames.AdminSchema);
        builder.Property(x => x.PermissionCode).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.RoleId, x.PermissionCode }).IsUnique();
    }
}

internal sealed class RoleDataScopeConfiguration : IEntityTypeConfiguration<RoleDataScope>
{
    public void Configure(EntityTypeBuilder<RoleDataScope> builder)
    {
        builder.ToTable(TableNames.RoleDataScopes, TableNames.AdminSchema);
        builder.Property(x => x.Module).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Scope).HasConversion<string>().HasMaxLength(50);
        builder.HasIndex(x => new { x.RoleId, x.Module }).IsUnique();
    }
}

internal sealed class CatalogItemConfiguration : IEntityTypeConfiguration<CatalogItem>
{
    public void Configure(EntityTypeBuilder<CatalogItem> builder)
    {
        builder.ToTable(TableNames.CatalogItems, TableNames.AdminSchema);
        builder.Property(x => x.Type).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => new { x.TenantId, x.Type, x.Code }).IsUnique().HasFilter("is_deleted = false");
    }
}
