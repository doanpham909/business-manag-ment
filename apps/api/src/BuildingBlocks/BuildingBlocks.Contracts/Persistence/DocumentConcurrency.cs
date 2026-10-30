using BuildingBlocks.Contracts.Domain.Exceptions;

namespace BuildingBlocks.Contracts.Persistence;

/// <summary>
/// Entity có phiên bản để chống sửa đè đồng thời (CMN-10). Version ánh xạ cột hệ thống xmin của PostgreSQL:
/// client gửi lại Version đã đọc khi cập nhật; khác phiên bản hiện tại → 409, người dùng tải lại.
/// </summary>
public interface IVersioned
{
    uint Version { get; }
}

public sealed class ConcurrencyConflictException()
    : ConflictException("Dữ liệu đã được người khác thay đổi. Vui lòng tải lại và thử lại.");

public static class ConcurrencyGuard
{
    /// <summary>Ném 409 khi client gửi phiên bản khác với phiên bản đang lưu.</summary>
    public static void EnsureVersion(this IVersioned entity, uint? expectedVersion)
    {
        if (expectedVersion.HasValue && expectedVersion.Value != entity.Version)
            throw new ConcurrencyConflictException();
    }
}

/// <summary>
/// Sinh số chứng từ tăng dần theo công ty, loại chứng từ và tháng (CMN-04), ví dụ PN-202610-0001.
/// Cấp số nguyên tử ở DB; nếu lệnh lưu chứng từ thất bại sau khi đã cấp, số đó bị bỏ qua (có thể nhảy số).
/// </summary>
public interface IDocumentNumberGenerator
{
    Task<string> NextAsync(string documentType, string prefix, DateOnly documentDate, CancellationToken cancellationToken);
}
