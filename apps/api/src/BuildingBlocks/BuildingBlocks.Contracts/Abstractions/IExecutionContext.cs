namespace BuildingBlocks.Contracts.Abstractions;

/// <summary>Người dùng hiện tại (đọc từ claim "sub"). Null khi request ẩn danh / background job.</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }
}

/// <summary>
/// Tenant (công ty) của request hiện tại (đọc từ claim "tenant_id").
/// Null → DbContext lọc ra rỗng với entity ITenantEntity (fail-closed) và không cho insert.
/// </summary>
public interface ITenantContext
{
    Guid? TenantId { get; }
}
