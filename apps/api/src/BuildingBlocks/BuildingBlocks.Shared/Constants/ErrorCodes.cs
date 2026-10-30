namespace BuildingBlocks.Shared;

/// <summary>Mã lỗi chuẩn (field <c>code</c> trong envelope lỗi) để FE phân biệt trường hợp lỗi.</summary>
public static class ErrorCodes
{
    public const string Validation = "VALIDATION_ERROR";
    public const string BusinessRule = "BUSINESS_RULE_VIOLATION";
    public const string NotFound = "NOT_FOUND";
    public const string BadRequest = "BAD_REQUEST";
    public const string Conflict = "CONFLICT";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string ServerError = "SERVER_ERROR";
    public const string NullValue = "NULL_VALUE";
}
