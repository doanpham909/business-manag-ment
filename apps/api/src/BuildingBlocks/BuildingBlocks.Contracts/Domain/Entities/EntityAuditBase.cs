namespace BuildingBlocks.Contracts.Domain.Entities;

/// <summary>
/// Entity có audit + xóa mềm. Setter private: chỉ EF/interceptor được gán (qua change tracker),
/// code nghiệp vụ không tự sửa các field audit.
/// </summary>
public abstract class EntityAuditBase<TKey> : EntityBase<TKey>, IAuditable, ISoftDelete
    where TKey : notnull
{
    public DateTimeOffset CreatedAt { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public Guid? DeletedBy { get; private set; }

    /// <summary>Khôi phục bản ghi đã xóa mềm (cần load bằng IgnoreQueryFilters trước).</summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
