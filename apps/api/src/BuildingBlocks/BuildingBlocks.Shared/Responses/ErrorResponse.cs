namespace BuildingBlocks.Shared.Responses;

/// <summary>Chi tiết một lỗi (thường theo field).</summary>
public sealed record ErrorDetail(string Code, string Message);

/// <summary>
/// Envelope lỗi thống nhất (giống ExceptionHandlingMiddleware của QMS, thêm traceId).
/// Dùng chung cho global exception handler và ApiControllerBase.HandleFailure.
/// </summary>
public sealed record ErrorResponse(
    string? Code,
    string Type,
    string Title,
    int Status,
    string Detail,
    IReadOnlyCollection<ErrorDetail>? Errors,
    string? TraceId);
