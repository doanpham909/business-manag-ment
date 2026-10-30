using Bms.Domain.Admin.Roles;

namespace Bms.Application.Authorization;

/// <summary>Vai trò mẫu tạo sẵn cho khách hàng mới (04-permission-matrix.md §4–5). Khách hàng sửa được.</summary>
public sealed record RoleTemplate(
    string Code,
    string Name,
    string Description,
    IReadOnlyDictionary<string, string> Grants,
    IReadOnlyDictionary<string, DataScope> Scopes,
    bool IsSystem = false)
{
    public IEnumerable<string> PermissionCodes => IsSystem
        ? PermissionCatalog.AllCodes.Where(c => !c.EndsWith(".view-sensitive", StringComparison.Ordinal) || c.StartsWith("admin.", StringComparison.Ordinal))
        : Grants.SelectMany(g => PermissionCatalog.Expand(g.Key, g.Value)).Distinct();
}

public static class RoleTemplates
{
    public const string TenantAdmin = "tenant_admin";

    private static Dictionary<string, DataScope> AllModules(DataScope scope) =>
        PermissionCatalog.ModuleNames.Keys.ToDictionary(m => m, _ => scope);

    public static readonly IReadOnlyList<RoleTemplate> All =
    [
        new(TenantAdmin, "Quản trị hệ thống", "Toàn quyền quản trị (không mặc định xem lương).",
            new Dictionary<string, string>(), AllModules(DataScope.All), IsSystem: true),
        new("leadership", "Ban lãnh đạo", "Xem và duyệt toàn công ty; xem chế độ Tất cả công ty.",
            new Dictionary<string, string>
            {
                ["admin.company"] = "VW", ["admin.user"] = "V", ["admin.catalog"] = "V", ["admin.auditlog"] = "V",
                ["hrm.department"] = "V", ["hrm.position"] = "V", ["hrm.employee"] = "VE",
                ["payroll.schedule"] = "V", ["payroll.attendance"] = "V", ["payroll.timesheet"] = "VA",
                ["payroll.payroll"] = "VAN", ["payroll.settlement"] = "VA",
                ["accounting.budget"] = "VA", ["accounting.cost"] = "V", ["accounting.payment-request"] = "VA",
                ["accounting.payment"] = "V", ["accounting.advance"] = "VA", ["accounting.debt"] = "VE",
                ["accounting.revenue"] = "VE", ["accounting.cashflow"] = "VE", ["accounting.journal"] = "V",
                ["accounting.report"] = "VE", ["inventory.material"] = "V", ["inventory.supplier"] = "V",
                ["inventory.warehouse"] = "V", ["inventory.material-request"] = "V", ["inventory.purchase"] = "VA",
                ["inventory.receipt"] = "V", ["inventory.issue"] = "V", ["inventory.transfer"] = "V",
                ["inventory.stock"] = "VE", ["inventory.stocktake"] = "VA", ["projects.project"] = "VE",
            }, AllModules(DataScope.All)),
        new("dept_head", "Trưởng phòng / đơn vị", "Quản lý dữ liệu của đơn vị mình và đơn vị cấp dưới.",
            new Dictionary<string, string>
            {
                ["admin.catalog"] = "V", ["hrm.department"] = "V", ["hrm.position"] = "V", ["hrm.employee"] = "V",
                ["payroll.schedule"] = "VCU", ["payroll.attendance"] = "VCUSA", ["payroll.timesheet"] = "VUSA",
                ["payroll.payroll"] = "V", ["accounting.budget"] = "VCS", ["accounting.cost"] = "V",
                ["accounting.payment-request"] = "VCUSA", ["accounting.advance"] = "VCUSA", ["accounting.report"] = "V",
                ["inventory.material"] = "V", ["inventory.supplier"] = "V", ["inventory.material-request"] = "VCUSA",
                ["inventory.purchase"] = "V", ["inventory.stock"] = "V", ["projects.project"] = "V",
            }, AllModules(DataScope.DepartmentTree)),
        new("hr_officer", "Chuyên viên nhân sự", "Quản lý cơ cấu tổ chức và hồ sơ nhân viên.",
            new Dictionary<string, string>
            {
                ["admin.catalog"] = "V", ["hrm.department"] = "VCUDIE", ["hrm.position"] = "VCUDIE",
                ["hrm.employee"] = "VCUDIEN", ["payroll.schedule"] = "V", ["payroll.attendance"] = "V",
                ["payroll.timesheet"] = "V", ["accounting.payment-request"] = "VCUS",
            }, new Dictionary<string, DataScope> { ["hrm"] = DataScope.All, ["accounting"] = DataScope.Own }),
        new("payroll_officer", "Chuyên viên tiền lương", "Chấm công, bảng công, bảng lương.",
            new Dictionary<string, string>
            {
                ["admin.catalog"] = "V", ["hrm.department"] = "V", ["hrm.position"] = "V", ["hrm.employee"] = "VN",
                ["payroll.schedule"] = "VCUDI", ["payroll.attendance"] = "VCUDIE", ["payroll.timesheet"] = "VCUSEL",
                ["payroll.payroll"] = "VCUSELN", ["payroll.payslip"] = "VCE", ["payroll.settlement"] = "VCUSE",
                ["accounting.payment-request"] = "VCUS",
            }, new Dictionary<string, DataScope> { ["hrm"] = DataScope.All, ["payroll"] = DataScope.All, ["accounting"] = DataScope.Own }),
        new("accountant", "Kế toán viên", "Ghi nhận chi phí, thanh toán, công nợ, hạch toán.",
            new Dictionary<string, string>
            {
                ["admin.catalog"] = "VCU", ["hrm.department"] = "V", ["hrm.employee"] = "V", ["payroll.payroll"] = "V",
                ["payroll.settlement"] = "V", ["accounting.budget"] = "VCUSIE", ["accounting.cost"] = "VCUIE",
                ["accounting.payment-request"] = "VCUSE", ["accounting.payment"] = "VCU", ["accounting.advance"] = "VCUSE",
                ["accounting.debt"] = "VCUE", ["accounting.revenue"] = "VCUE", ["accounting.cashflow"] = "VCUE",
                ["accounting.journal"] = "VCUIE", ["accounting.report"] = "VE", ["inventory.material"] = "V",
                ["inventory.supplier"] = "V", ["inventory.warehouse"] = "V", ["inventory.purchase"] = "V",
                ["inventory.receipt"] = "V", ["inventory.issue"] = "V", ["inventory.stock"] = "VE",
                ["inventory.stocktake"] = "V", ["projects.project"] = "V",
            }, AllModules(DataScope.All)),
        new("chief_accountant", "Kế toán trưởng", "Duyệt và khóa sổ kế toán.",
            new Dictionary<string, string>
            {
                ["admin.catalog"] = "VCUD", ["admin.auditlog"] = "V", ["hrm.department"] = "V", ["hrm.employee"] = "V",
                ["payroll.payroll"] = "VAN", ["payroll.settlement"] = "VAL", ["accounting.budget"] = "VAL",
                ["accounting.cost"] = "VCUDA", ["accounting.payment-request"] = "VAE", ["accounting.payment"] = "VCUA",
                ["accounting.advance"] = "VAE", ["accounting.debt"] = "VCUAE", ["accounting.revenue"] = "VCUAE",
                ["accounting.cashflow"] = "VCUAE", ["accounting.journal"] = "VCUDAEL", ["accounting.report"] = "VE",
                ["inventory.material"] = "V", ["inventory.supplier"] = "V", ["inventory.warehouse"] = "V",
                ["inventory.purchase"] = "VA", ["inventory.receipt"] = "V", ["inventory.issue"] = "V",
                ["inventory.stock"] = "VE", ["inventory.stocktake"] = "VA", ["projects.project"] = "V",
            }, AllModules(DataScope.All)),
        new("purchaser", "Nhân viên mua hàng", "Nhà cung cấp, yêu cầu mua, đơn mua.",
            new Dictionary<string, string>
            {
                ["admin.catalog"] = "V", ["hrm.department"] = "V", ["accounting.payment-request"] = "VCUS",
                ["inventory.material"] = "VCUIE", ["inventory.supplier"] = "VCUIE", ["inventory.warehouse"] = "V",
                ["inventory.material-request"] = "V", ["inventory.purchase"] = "VCUSE", ["inventory.receipt"] = "V",
                ["inventory.issue"] = "V", ["inventory.transfer"] = "V", ["inventory.stock"] = "VE",
                ["inventory.stocktake"] = "V", ["projects.project"] = "V",
            }, new Dictionary<string, DataScope> { ["inventory"] = DataScope.Project, ["accounting"] = DataScope.Own }),
        new("storekeeper", "Thủ kho", "Nhập, xuất, chuyển kho, kiểm kê.",
            new Dictionary<string, string>
            {
                ["admin.catalog"] = "V", ["hrm.department"] = "V", ["accounting.payment-request"] = "VCUS",
                ["inventory.material"] = "VCU", ["inventory.supplier"] = "VCU", ["inventory.warehouse"] = "VU",
                ["inventory.material-request"] = "V", ["inventory.receipt"] = "VCUAE", ["inventory.issue"] = "VCUAE",
                ["inventory.transfer"] = "VCUSA", ["inventory.stock"] = "VE", ["inventory.stocktake"] = "VCUIS",
                ["projects.project"] = "V",
            }, new Dictionary<string, DataScope> { ["inventory"] = DataScope.Project, ["accounting"] = DataScope.Own }),
        new("project_manager", "Ban điều hành dự án", "Quản lý dữ liệu của dự án được gán.",
            new Dictionary<string, string>
            {
                ["admin.catalog"] = "V", ["hrm.department"] = "V", ["hrm.employee"] = "V",
                ["payroll.schedule"] = "VCU", ["payroll.attendance"] = "VCUS", ["payroll.timesheet"] = "VUS",
                ["accounting.budget"] = "VCS", ["accounting.cost"] = "V", ["accounting.payment-request"] = "VCUSA",
                ["accounting.advance"] = "VCUSA", ["accounting.report"] = "VE", ["inventory.material"] = "V",
                ["inventory.supplier"] = "V", ["inventory.warehouse"] = "VCU", ["inventory.material-request"] = "VCUSA",
                ["inventory.purchase"] = "VCUSA", ["inventory.receipt"] = "V", ["inventory.issue"] = "V",
                ["inventory.transfer"] = "VA", ["inventory.stock"] = "VE", ["inventory.stocktake"] = "VA",
                ["projects.project"] = "VCUDIE",
            }, AllModules(DataScope.Project)),
        new("employee", "Nhân viên", "Quyền mặc định của mọi người dùng: dữ liệu của chính mình.",
            new Dictionary<string, string>
            {
                ["admin.catalog"] = "V", ["hrm.department"] = "V", ["hrm.employee"] = "V",
                ["payroll.attendance"] = "VC", ["payroll.timesheet"] = "V", ["payroll.payslip"] = "V",
                ["accounting.payment-request"] = "VCUS", ["accounting.advance"] = "VCUS",
                ["inventory.material-request"] = "VCUS",
            }, AllModules(DataScope.Own)),
    ];
}
