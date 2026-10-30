using BuildingBlocks.Contracts.Domain.Entities;

namespace BuildingBlocks.Contracts.Auditing;

/// <summary>
/// Nhật ký thay đổi dữ liệu (CMN-03): ai, khi nào, tenant/công ty nào, bản ghi nào, giá trị trước/sau.
/// Do AuditLogInterceptor (BuildingBlocks.Infrastructure) ghi tự động, không sửa, không xóa.
/// </summary>
public sealed class AuditLog : EntityBase<Guid>, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? UserId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public AuditAction Action { get; private set; }

    /// <summary>JSON: { "Thuộc tính": { "old": ..., "new": ... } }</summary>
    public string Changes { get; private set; } = "{}";

    public DateTimeOffset OccurredAt { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        Guid tenantId, Guid? companyId, Guid? userId, string entityType, string entityId,
        AuditAction action, string changes, DateTimeOffset occurredAt) => new()
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            CompanyId = companyId,
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Changes = changes,
            OccurredAt = occurredAt,
        };
}

public enum AuditAction
{
    Created,
    Updated,
    Deleted,
}
