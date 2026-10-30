using Bms.Domain.Hrm.Departments;
using Bms.Domain.Projects;
using Bms.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bms.Persistence.Configurations.Business;

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable(TableNames.Departments, TableNames.HrmSchema);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Path).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.TypeCode).HasMaxLength(50);
        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.Code }).IsUnique().HasFilter("is_deleted = false");
        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.ParentId });
    }
}

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable(TableNames.Projects, TableNames.ProjectsSchema);
        builder.Ignore(x => x.AcceptsNewDocuments);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ProjectTypeCode).HasMaxLength(50);
        builder.Property(x => x.ConstructionTypeCode).HasMaxLength(50);
        builder.Property(x => x.Location).HasMaxLength(500);
        builder.Property(x => x.ManagerName).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.Code }).IsUnique().HasFilter("is_deleted = false");
        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.ParentProjectId });
    }
}
