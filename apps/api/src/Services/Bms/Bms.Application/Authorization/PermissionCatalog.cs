namespace Bms.Application.Authorization;

/// <summary>Một nhóm quyền theo tài nguyên, dùng cho màn hình cấu hình vai trò.</summary>
public sealed record PermissionResource(string Module, string Code, string Name, IReadOnlyList<PermissionItem> Items);

public sealed record PermissionItem(string Code, string Action, string ActionName);

/// <summary>
/// Danh mục quyền tập trung (docs/analysis/04-permission-matrix.md). Mã quyền: {module}.{resource}.{action}.
/// Ký hiệu hành động: V xem · C thêm · U sửa · D xóa · S gửi duyệt · A duyệt/từ chối · I nhập Excel ·
/// E xuất · L khóa kỳ · N xem thông tin nhạy cảm · W xem Tất cả công ty.
/// </summary>
public static class PermissionCatalog
{
    private static readonly Dictionary<char, (string Action, string Name)> Actions = new()
    {
        ['V'] = ("view", "Xem"),
        ['C'] = ("create", "Thêm"),
        ['U'] = ("update", "Sửa"),
        ['D'] = ("delete", "Xóa"),
        ['S'] = ("submit", "Gửi duyệt"),
        ['A'] = ("approve", "Duyệt / từ chối"),
        ['I'] = ("import", "Nhập Excel"),
        ['E'] = ("export", "Xuất Excel"),
        ['L'] = ("lock", "Khóa / mở khóa"),
        ['N'] = ("view-sensitive", "Xem thông tin nhạy cảm"),
        ['W'] = ("view-all", "Xem Tất cả công ty"),
    };

    public static readonly IReadOnlyDictionary<string, string> ModuleNames = new Dictionary<string, string>
    {
        ["admin"] = "Quản trị hệ thống",
        ["hrm"] = "Tổ chức – Nhân sự",
        ["payroll"] = "Chấm công – Tính lương",
        ["accounting"] = "Tài chính – Kế toán",
        ["inventory"] = "Quản lý kho",
        ["projects"] = "Quản lý dự án",
    };

    private static readonly (string Code, string Name, string Actions)[] Definitions =
    [
        ("admin.company", "Công ty", "VCUDW"),
        ("admin.user", "Người dùng", "VCUDE"),
        ("admin.role", "Vai trò và phân quyền", "VCUD"),
        ("admin.catalog", "Danh mục dùng chung", "VCUDIE"),
        ("admin.workflow", "Quy trình duyệt", "VCU"),
        ("admin.setting", "Cấu hình hệ thống", "VU"),
        ("admin.auditlog", "Nhật ký thay đổi", "VE"),
        ("hrm.department", "Cơ cấu tổ chức", "VCUDIE"),
        ("hrm.position", "Vị trí, chức danh", "VCUDIE"),
        ("hrm.employee", "Hồ sơ nhân viên", "VCUDIEN"),
        ("payroll.schedule", "Lịch làm việc, ca", "VCUDI"),
        ("payroll.attendance", "Chấm công", "VCUDSAIE"),
        ("payroll.timesheet", "Bảng công tháng", "VCUSAEL"),
        ("payroll.payroll", "Bảng lương", "VCUSAELN"),
        ("payroll.payslip", "Phiếu lương", "VCE"),
        ("payroll.settlement", "Quyết toán lương", "VCUSAEL"),
        ("accounting.budget", "Ngân sách", "VCUSAIEL"),
        ("accounting.cost", "Chi phí", "VCUDAIE"),
        ("accounting.payment-request", "Đề nghị thanh toán", "VCUDSAE"),
        ("accounting.payment", "Chi tiền", "VCUA"),
        ("accounting.advance", "Tạm ứng", "VCUDSAE"),
        ("accounting.debt", "Công nợ", "VCUAE"),
        ("accounting.revenue", "Doanh thu", "VCUAE"),
        ("accounting.cashflow", "Dòng tiền", "VCUAE"),
        ("accounting.journal", "Hạch toán", "VCUDAIEL"),
        ("accounting.report", "Báo cáo tài chính", "VE"),
        ("inventory.material", "Vật tư", "VCUDIE"),
        ("inventory.supplier", "Nhà cung cấp", "VCUDIE"),
        ("inventory.warehouse", "Kho", "VCUD"),
        ("inventory.material-request", "Yêu cầu vật tư", "VCUDSA"),
        ("inventory.purchase", "Mua hàng", "VCUDSAE"),
        ("inventory.receipt", "Phiếu nhập kho", "VCUDAE"),
        ("inventory.issue", "Phiếu xuất kho", "VCUDAE"),
        ("inventory.transfer", "Chuyển kho", "VCUDSA"),
        ("inventory.stock", "Tồn kho, thẻ kho", "VE"),
        ("inventory.stocktake", "Kiểm kê", "VCUSAIL"),
        ("projects.project", "Dự án", "VCUDIE"),
    ];

    public static readonly IReadOnlyList<PermissionResource> Resources = Definitions
        .Select(d =>
        {
            var module = d.Code[..d.Code.IndexOf('.', StringComparison.Ordinal)];
            var items = d.Actions.Select(c => new PermissionItem($"{d.Code}.{Actions[c].Action}", Actions[c].Action, Actions[c].Name));
            return new PermissionResource(module, d.Code, d.Name, items.ToList());
        })
        .ToList();

    public static readonly IReadOnlySet<string> AllCodes =
        Resources.SelectMany(r => r.Items).Select(i => i.Code).ToHashSet();

    /// <summary>Đổi ký hiệu ("VCU") của một tài nguyên thành mã quyền; bỏ ký hiệu không có trong danh mục.</summary>
    public static IEnumerable<string> Expand(string resource, string letters) =>
        letters.Where(Actions.ContainsKey)
            .Select(c => $"{resource}.{Actions[c].Action}")
            .Where(AllCodes.Contains);
}
