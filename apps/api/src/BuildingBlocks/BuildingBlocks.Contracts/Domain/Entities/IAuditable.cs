namespace BuildingBlocks.Contracts.Domain.Entities;

/// <summary>
/// Theo dõi người/thời điểm tạo &amp; cập nhật. Giá trị do AuditableEntityInterceptor gán tự động
/// (UTC, <see cref="DateTimeOffset"/> offset 0 → cột timestamptz của PostgreSQL).
/// </summary>
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; }

    Guid? CreatedBy { get; }

    DateTimeOffset? UpdatedAt { get; }

    Guid? UpdatedBy { get; }
}

/// <summary>
/// Xóa mềm. Gọi Remove() trên repository/DbContext → interceptor chuyển thành UPDATE is_deleted = true.
/// Bản ghi đã xóa mềm bị ẩn bởi global query filter "SoftDelete".
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; }

    DateTimeOffset? DeletedAt { get; }

    Guid? DeletedBy { get; }
}

/// <summary>
/// Entity thuộc về một tenant (công ty). TenantId được interceptor gán khi insert từ ITenantContext,
/// và DbContext tự lọc theo tenant hiện tại qua global query filter "Tenant".
/// </summary>
public interface ITenantEntity
{
    Guid TenantId { get; }
}
