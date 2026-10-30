namespace BuildingBlocks.Contracts.Persistence;

/// <summary>
/// Tên các global query filter (EF Core 10 named filters) áp cho mọi entity tương ứng.
/// <para>
/// Bỏ qua filter cho thao tác hệ thống / quản trị (CHỈ dùng có chủ đích, phải kiểm tra quyền trước):
/// <code>
/// // Đọc dữ liệu của mọi tenant (vd: super-admin, job hệ thống) — vẫn ẩn bản ghi đã xóa mềm
/// query.IgnoreQueryFilters([QueryFilterNames.Tenant]);
/// // Xem cả bản ghi đã xóa mềm (vd: chức năng khôi phục) — vẫn giới hạn trong tenant hiện tại
/// query.IgnoreQueryFilters([QueryFilterNames.SoftDelete]);
/// // Bỏ toàn bộ filter
/// query.IgnoreQueryFilters();
/// </code>
/// </para>
/// </summary>
public static class QueryFilterNames
{
    public const string Tenant = "Tenant";
    public const string SoftDelete = "SoftDelete";

    /// <summary>Lọc theo công ty được đọc (ADR-0002) cho entity ICompanyEntity.</summary>
    public const string Company = "Company";
}
