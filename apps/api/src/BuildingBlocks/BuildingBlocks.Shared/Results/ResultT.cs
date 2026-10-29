using System.Text.Json.Serialization;

namespace BuildingBlocks.Shared.Results;

/// <summary>Kết quả có kèm dữ liệu trả về (field <c>value</c> trong JSON envelope).</summary>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error, string? warningMessage = null)
        : base(isSuccess, error, warningMessage)
        => _value = value;

    /// <summary>Dữ liệu trả về; khi thất bại trả default (null) để serialize an toàn.</summary>
    /// <remarks>Dùng <see cref="GetValueOrThrow"/> trong code khi muốn chắc chắn đã thành công.</remarks>
    [JsonPropertyOrder(1)]
    public TValue? Value => IsSuccess ? _value : default;

    /// <summary>Lấy dữ liệu, ném lỗi nếu result thất bại (tránh dùng nhầm giá trị rỗng).</summary>
    public TValue GetValueOrThrow() => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    public static implicit operator Result<TValue>(TValue? value) => Create(value);
}
