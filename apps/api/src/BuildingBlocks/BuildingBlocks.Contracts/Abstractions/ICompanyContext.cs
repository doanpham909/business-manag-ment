namespace BuildingBlocks.Contracts.Abstractions;

/// <summary>
/// Công ty (pháp nhân) của request hiện tại — ADR-0002. Đọc từ claim của user đã xác thực,
/// không bao giờ từ dữ liệu client tự gửi.
/// </summary>
public interface ICompanyContext
{
    /// <summary>Công ty đang làm việc. Null khi chưa chọn công ty hoặc đang ở chế độ "Tất cả công ty".</summary>
    Guid? ActiveCompanyId { get; }

    /// <summary>
    /// Các công ty được đọc: [ActiveCompanyId] khi đã chọn công ty; danh sách được phép khi ở chế độ
    /// "Tất cả công ty" (chỉ đọc); rỗng khi chưa chọn (fail-closed).
    /// </summary>
    IReadOnlyList<Guid> ReadableCompanyIds { get; }

    /// <summary>True khi đang ở chế độ "Tất cả công ty" (chỉ đọc).</summary>
    bool IsAllCompanies { get; }
}
