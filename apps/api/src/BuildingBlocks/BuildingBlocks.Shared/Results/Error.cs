namespace BuildingBlocks.Shared.Results;

/// <summary>
/// Lỗi nghiệp vụ dạng giá trị dùng trong <see cref="Result"/>.
/// Khác QMS: không kế thừa Exception, để JSON trả về chỉ gồm { code, message }.
/// </summary>
public sealed record Error(string Code, string Message)
{
    /// <summary>Không có lỗi — dùng cho kết quả thành công.</summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>Giá trị trả về bị null.</summary>
    public static readonly Error NullValue = new(ErrorCodes.NullValue, "The specified result value is null.");
}
