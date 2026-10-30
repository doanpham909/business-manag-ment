using Bms.Application.Authorization;
using Bms.Domain.Admin.Catalogs;
using Bms.Domain.Admin.Companies;
using Bms.Domain.Admin.Roles;
using Bms.Domain.Admin.Tenants;
using Bms.Domain.Admin.Users;
using Bms.Domain.Hrm.Departments;
using Bms.Domain.Projects;
using Bms.Persistence;
using BuildingBlocks.Contracts.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Bms.API.Seeding;

/// <summary>
/// CHỈ chạy ở môi trường Development (DEC-004): áp migration rồi nạp dữ liệu mẫu nếu DB còn trống.
/// Không có người thật. Mật khẩu mẫu dùng chung cho mọi tài khoản demo — không dùng ở môi trường khác.
/// </summary>
public static class DevelopmentSeeder
{
    public const string DemoPassword = "Demo@123";

    private static readonly (string Username, string FullName, string Role, bool BothCompanies)[] DemoUsers =
    [
        ("admin", "Quản trị hệ thống", RoleTemplates.TenantAdmin, true),
        ("lanhdao", "Nguyễn Văn Lãnh Đạo", "leadership", true),
        ("truongphong", "Trần Thị Trưởng Phòng", "dept_head", false),
        ("nhansu", "Lê Thị Nhân Sự", "hr_officer", false),
        ("tienluong", "Phạm Văn Tiền Lương", "payroll_officer", false),
        ("ketoan", "Hoàng Thị Kế Toán", "accountant", false),
        ("ketoantruong", "Vũ Văn Kế Toán Trưởng", "chief_accountant", false),
        ("muahang", "Đỗ Văn Mua Hàng", "purchaser", false),
        ("thukho", "Bùi Văn Thủ Kho", "storekeeper", false),
        ("chihuy", "Đặng Văn Chỉ Huy", "project_manager", false),
        ("nhanvien", "Ngô Thị Nhân Viên", "employee", false),
    ];

    public static async Task MigrateAndSeedDevelopmentDataAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<BmsDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await db.Database.MigrateAsync();
        if (await db.Set<Tenant>().AnyAsync())
            return;

        var tenant = Tenant.Create("DEMO", "Tập đoàn Xây dựng Demo");
        var companies = new[]
        {
            Company.Create("XD01", "Công ty CP Xây dựng Demo", "0100000001", "Hà Nội", "024 0000 0001", tenant.Id),
            Company.Create("CD02", "Công ty TNHH Cơ điện Demo", "0100000002", "TP. Hồ Chí Minh", "028 0000 0002", tenant.Id),
        };

        var roles = RoleTemplates.All.ToDictionary(t => t.Code, t =>
        {
            var role = Role.Create(t.Code, t.Name, t.Description, t.IsSystem, tenant.Id);
            role.SetPermissions(t.PermissionCodes);
            role.SetDataScopes(t.Scopes.Select(s => (s.Key, s.Value)));
            return role;
        });

        var passwordHash = hasher.Hash(DemoPassword);
        var users = DemoUsers.Select(d =>
        {
            var user = User.Create(d.Username, d.FullName, $"{d.Username}@demo.local", null, passwordHash, tenant.Id);
            var access = d.BothCompanies ? companies : companies[..1];
            user.SetCompanyAccess(access.Select(c => c.Id));
            foreach (var company in access)
                user.SetRoles(company.Id, [roles[d.Role].Id]);
            return user;
        });

        db.AddRange(tenant);
        db.AddRange(companies);
        db.AddRange(roles.Values);
        db.AddRange(users);
        SeedCatalogs(db, tenant.Id);
        SeedOrganization(db, tenant.Id, companies[0].Id);
        await db.SaveChangesAsync();
        app.Logger.LogInformation("Seeded development demo data");
    }

    private static void SeedCatalogs(BmsDbContext db, Guid tenantId)
    {
        (string Type, string Code, string Name)[] items =
        [
            (CatalogTypes.Unit, "CAI", "Cái"), (CatalogTypes.Unit, "KG", "Kilôgam"), (CatalogTypes.Unit, "TAN", "Tấn"),
            (CatalogTypes.Unit, "M", "Mét"), (CatalogTypes.Unit, "M2", "Mét vuông"), (CatalogTypes.Unit, "M3", "Mét khối"),
            (CatalogTypes.ProjectType, "DAUTU", "Dự án đầu tư"), (CatalogTypes.ProjectType, "THAUPHU", "Dự án thầu phụ"),
            (CatalogTypes.ConstructionType, "DANDUNG", "Dân dụng"), (CatalogTypes.ConstructionType, "CONGNGHIEP", "Công nghiệp"),
            (CatalogTypes.ConstructionType, "HATANG", "Hạ tầng"),
            (CatalogTypes.DepartmentType, "BAN", "Ban"), (CatalogTypes.DepartmentType, "PHONG", "Phòng"),
            (CatalogTypes.DepartmentType, "CONGTRUONG", "Công trường"),
            (CatalogTypes.CostGroup, "VATTU", "Chi phí vật tư"), (CatalogTypes.CostGroup, "NHANCONG", "Chi phí nhân công"),
            (CatalogTypes.CostGroup, "MAY", "Chi phí máy"), (CatalogTypes.CostGroup, "CHUNG", "Chi phí chung"),
            (CatalogTypes.MaterialGroup, "THEP", "Thép"), (CatalogTypes.MaterialGroup, "XIMANG", "Xi măng"),
            (CatalogTypes.Bank, "VCB", "Vietcombank"), (CatalogTypes.Bank, "BIDV", "BIDV"),
        ];
        db.AddRange(items.Select((x, i) => CatalogItem.Create(x.Type, x.Code, x.Name, null, i, tenantId)));
    }

    private static void SeedOrganization(BmsDbContext db, Guid tenantId, Guid companyId)
    {
        T Stamp<T>(T entity) where T : class
        {
            db.Add(entity);
            db.Entry(entity).Property("TenantId").CurrentValue = tenantId;
            db.Entry(entity).Property("CompanyId").CurrentValue = companyId;
            return entity;
        }

        var board = Stamp(Department.Create("BGD", "Ban Giám đốc", null, "BAN", 0));
        Stamp(Department.Create("PKT", "Phòng Kế toán", board, "PHONG", 1));
        Stamp(Department.Create("PNS", "Phòng Nhân sự", board, "PHONG", 2));
        var site = Stamp(Department.Create("BCH01", "Ban chỉ huy công trường Tòa A", board, "CONGTRUONG", 3));

        var tower = Stamp(Project.Create("DA-TOA-A", new ProjectInfo("Tòa nhà văn phòng A", null, "DAUTU", "DANDUNG",
            "Hà Nội", site.Id, "Đặng Văn Chỉ Huy", new DateOnly(2026, 1, 1), new DateOnly(2027, 12, 31), null)));
        Stamp(Project.Create("DA-TOA-A-GT1", new ProjectInfo("Gói thầu móng cọc", tower.Id, "THAUPHU", "DANDUNG",
            "Hà Nội", site.Id, "Đặng Văn Chỉ Huy", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), null)));
        Stamp(Project.Create("DA-NM-B", new ProjectInfo("Nhà máy B", null, "DAUTU", "CONGNGHIEP",
            "Bắc Ninh", null, null, new DateOnly(2026, 3, 1), new DateOnly(2027, 6, 30), null)));
    }
}
