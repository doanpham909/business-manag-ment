using Bms.Application.Authorization;
using Bms.Domain.Admin.Companies;
using Bms.Domain.Admin.Roles;
using Bms.Domain.Admin.Tenants;
using Bms.Domain.Admin.Users;
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
        await db.SaveChangesAsync();
        app.Logger.LogInformation("Seeded development demo data (password for all demo users: {Password})", DemoPassword);
    }
}
