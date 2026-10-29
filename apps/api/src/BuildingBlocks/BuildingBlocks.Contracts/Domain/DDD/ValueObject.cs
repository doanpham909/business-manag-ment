namespace BuildingBlocks.Contracts.Domain.DDD;

/// <summary>
/// Value Object: bất biến, so sánh bằng giá trị. Đơn giản hơn QMS (không dùng reflection):
/// lớp con liệt kê các thành phần so sánh trong <see cref="GetEqualityComponents"/>.
/// Với VO đơn giản có thể dùng thẳng <c>record</c>.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other) =>
        other is not null
        && GetType() == other.GetType()
        && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override bool Equals(object? obj) => obj is ValueObject other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var component in GetEqualityComponents())
            hash.Add(component);
        return hash.ToHashCode();
    }

    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);

    /// <summary>Kiểm tra business rule khi khởi tạo VO.</summary>
    protected static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
            throw new BusinessRuleValidationException(rule);
    }
}
