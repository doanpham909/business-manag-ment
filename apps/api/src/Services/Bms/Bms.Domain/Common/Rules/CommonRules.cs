using BuildingBlocks.Contracts.Domain.DDD;

namespace Bms.Domain.Common.Rules;

/// <summary>Trường văn bản bắt buộc không được để trống (sau khi bỏ khoảng trắng).</summary>
public sealed class TextMustNotBeEmptyRule(string? value, string fieldName, string label) : IBusinessRule
{
    public bool IsBroken() => string.IsNullOrWhiteSpace(value);

    public string Message => $"{label} không được để trống.";

    public string FieldName => fieldName;
}

/// <summary>Ngày kết thúc không được trước ngày bắt đầu.</summary>
public sealed class EndDateMustNotBeBeforeStartDateRule(DateOnly? start, DateOnly? end, string fieldName) : IBusinessRule
{
    public bool IsBroken() => start.HasValue && end.HasValue && end.Value < start.Value;

    public string Message => "Ngày kết thúc không được trước ngày bắt đầu.";

    public string FieldName => fieldName;
}
