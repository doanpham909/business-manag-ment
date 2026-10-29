namespace BuildingBlocks.Contracts.Domain.DDD;

/// <summary>Business rule. Đặt tên: {Subject}Must{Condition}Rule.</summary>
public interface IBusinessRule
{
    bool IsBroken();

    string Message { get; }

    /// <summary>Tên field liên quan, dùng làm "code" trong errors[] của response (mặc định "Name" như QMS).</summary>
    string FieldName => "Name";
}

/// <summary>Ném ra khi business rule bị vi phạm → HTTP 422.</summary>
public sealed class BusinessRuleValidationException(IBusinessRule brokenRule) : Exception(brokenRule.Message)
{
    public IBusinessRule BrokenRule { get; } = brokenRule;

    public override string ToString() => $"{BrokenRule.GetType().FullName}: {BrokenRule.Message}";
}
