namespace BuildingBlocks.Shared.Results;

/// <summary>Đánh dấu một Result thất bại do validation, kèm danh sách lỗi chi tiết theo field.</summary>
public interface IValidationResult
{
    static readonly Error ValidationError = new(ErrorCodes.Validation, "A validation problem occurred.");

    Error[] Errors { get; }
}

/// <summary>Result thất bại do validation (không có dữ liệu).</summary>
public sealed class ValidationResult : Result, IValidationResult
{
    private ValidationResult(Error[] errors)
        : base(false, IValidationResult.ValidationError) => Errors = errors;

    public Error[] Errors { get; }

    public static ValidationResult WithErrors(params Error[] errors) => new(errors);
}

/// <summary>Result thất bại do validation (generic) — handler trả về khi validate nghiệp vụ.</summary>
public sealed class ValidationResult<TValue> : Result<TValue>, IValidationResult
{
    private ValidationResult(Error[] errors)
        : base(default, false, IValidationResult.ValidationError) => Errors = errors;

    public Error[] Errors { get; }

    public static ValidationResult<TValue> WithErrors(params Error[] errors) => new(errors);
}
