using BuildingBlocks.Contracts.Domain.DDD;
using BuildingBlocks.Contracts.Domain.Entities;

namespace Bms.Domain.Admin.Auth;

/// <summary>
/// Refresh token (lưu mã băm, không lưu token gốc). Xoay vòng mỗi lần làm mới.
/// Không phải ITenantEntity: được tra cứu khi chưa có access token (chưa có tenant trong ngữ cảnh);
/// TenantId lưu để biết token thuộc khách hàng nào.
/// </summary>
public sealed class RefreshToken : EntityBase<Guid>, IAggregateRoot, INotAudited
{
    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }

    /// <summary>Công ty đang làm việc khi cấp token. Null: chưa chọn hoặc chế độ Tất cả công ty.</summary>
    public Guid? CompanyId { get; private set; }

    public bool AllCompanies { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, Guid tenantId, Guid? companyId, bool allCompanies,
        string tokenHash, DateTimeOffset now, TimeSpan lifetime) => new()
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            TenantId = tenantId,
            CompanyId = companyId,
            AllCompanies = allCompanies,
            TokenHash = tokenHash,
            CreatedAt = now,
            ExpiresAt = now.Add(lifetime),
        };

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;

    public void Revoke(DateTimeOffset now) => RevokedAt ??= now;
}
