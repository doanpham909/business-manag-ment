using System.Data.Common;
using BuildingBlocks.Contracts.Domain.DDD;
using BuildingBlocks.Contracts.Domain.Exceptions;
using BuildingBlocks.Shared;
using BuildingBlocks.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Exceptions;

/// <summary>
/// Ánh xạ exception → HTTP status + <see cref="ErrorResponse"/> (theo ExceptionHandlingMiddleware của QMS):
/// ValidationException/BadRequest → 400, AccessDenied → 403, NotFound → 404, Conflict → 409,
/// BusinessRuleValidationException → 422, còn lại → 500 với message chung (KHÔNG lộ chi tiết nội bộ).
/// </summary>
internal static class ExceptionMapper
{
    /// <summary>Message trả cho mọi lỗi hệ thống không lường trước.</summary>
    internal const string GenericServerErrorMessage =
        "Đã có lỗi xảy ra, vui lòng thử lại hoặc liên hệ quản trị viên.";

    private const string ValidationDetail = "A validation problem occurred.";

    private const string UniqueViolationSqlState = "23505";

    private const string ConcurrencyMessage = "Dữ liệu đã được người khác thay đổi. Vui lòng tải lại và thử lại.";

    private const string DuplicateMessage = "Dữ liệu bị trùng với bản ghi đã có.";

    private static ErrorResponse Conflict(string message, string? traceId) => new(
        ErrorCodes.Conflict, "Conflict", "Conflict", StatusCodes.Status409Conflict, message, null, traceId);

    public static ErrorResponse Map(Exception exception, string? traceId) => exception switch
    {
        ValidationException validation => new ErrorResponse(
            validation.Code, "ValidationError", "Validation Error", StatusCodes.Status400BadRequest,
            ValidationDetail,
            validation.Errors.Select(e => new ErrorDetail(e.PropertyName, e.ErrorMessage)).ToArray(),
            traceId),

        BusinessRuleValidationException businessRule => new ErrorResponse(
            ErrorCodes.BusinessRule, "ValidationError", "Validation Error",
            StatusCodes.Status422UnprocessableEntity, ValidationDetail,
            [new ErrorDetail(businessRule.BrokenRule.FieldName, businessRule.BrokenRule.Message)],
            traceId),

        // Lỗi nghiệp vụ có message do ta tự soạn → trả nguyên văn cho người dùng
        DomainException domain => MapDomain(domain, traceId),

        // Optimistic concurrency (xmin): người khác đã sửa dòng này
        DbUpdateConcurrencyException => Conflict(ConcurrencyMessage, traceId),

        // Vi phạm unique index (PostgreSQL SqlState 23505) — lưới an toàn khi handler chưa kiểm tra trùng
        DbUpdateException { InnerException: DbException { SqlState: UniqueViolationSqlState } }
            => Conflict(DuplicateMessage, traceId),

        _ => new ErrorResponse(
            ErrorCodes.ServerError, "ServerError", "Server Error", StatusCodes.Status500InternalServerError,
            GenericServerErrorMessage, null, traceId),
    };

    private static ErrorResponse MapDomain(DomainException exception, string? traceId)
    {
        var (status, type) = exception switch
        {
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            AccessDeniedException => (StatusCodes.Status403Forbidden, "Forbidden"),
            NotFoundException => (StatusCodes.Status404NotFound, "NotFound"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status400BadRequest, "BadRequest"),
        };

        return new ErrorResponse(exception.Code, type, exception.Title, status, exception.Message, null, traceId);
    }
}
