using BuildingBlocks.Contracts.Domain.DDD;

namespace BuildingBlocks.Contracts.Domain.Entities;

/// <summary>Entity có định danh.</summary>
public interface IEntity<out TKey>
    where TKey : notnull
{
    TKey Id { get; }
}

/// <summary>
/// Lớp gốc của mọi entity. Id dạng Guid nên khởi tạo bằng <c>Guid.CreateVersion7()</c>
/// trong factory method (v7 tăng dần theo thời gian → index B-tree tốt hơn v4).
/// </summary>
public abstract class EntityBase<TKey> : IEntity<TKey>
    where TKey : notnull
{
    public TKey Id { get; protected set; } = default!;

    /// <summary>Kiểm tra business rule; vi phạm → <see cref="BusinessRuleValidationException"/> (HTTP 422).</summary>
    protected static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
            throw new BusinessRuleValidationException(rule);
    }
}
