using System.Text.Json.Serialization;

namespace BuildingBlocks.Shared.Results;

/// <summary>
/// Kết quả xử lý của một command/query (pattern Result giống QMS).
/// Khi serialize ra JSON chính là "response envelope" thành công trả cho FE.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error, string? warningMessage = null)
    {
        // Bảo vệ bất biến: thành công thì không có lỗi, thất bại thì bắt buộc có lỗi
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot contain an error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failed result must contain an error.");

        IsSuccess = isSuccess;
        Error = error;
        WarningMessage = warningMessage;
    }

    [JsonPropertyOrder(-3)]
    public bool IsSuccess { get; }

    [JsonPropertyOrder(-2)]
    public bool IsFailure => !IsSuccess;

    [JsonPropertyOrder(-1)]
    public Error Error { get; }

    public string? WarningMessage { get; }

    public static Result Success() => new(true, Error.None);

    public static Result SuccessWithWarning(string? warning) => new(true, Error.None, warning);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result<TValue> SuccessWithWarning<TValue>(TValue value, string? warning) =>
        new(value, true, Error.None, warning);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    /// <summary>Tạo Success nếu value khác null, ngược lại Failure(NullValue).</summary>
    public static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}
