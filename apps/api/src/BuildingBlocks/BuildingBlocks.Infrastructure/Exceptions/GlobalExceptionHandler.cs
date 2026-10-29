using System.Diagnostics;
using BuildingBlocks.Contracts.Domain.DDD;
using BuildingBlocks.Contracts.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Exceptions;

/// <summary>
/// Global exception handler (IExceptionHandler của ASP.NET Core, thay cho middleware tự viết của QMS).
/// Log đầy đủ ở server, trả <c>application/problem+json</c> theo shape ErrorResponse cho client.
/// Đăng ký: services.AddExceptionHandler&lt;GlobalExceptionHandler&gt;() + app.UseExceptionHandler().
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Client tự hủy request: không phải lỗi hệ thống, không cần ghi body
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation("Request {Path} was cancelled by the client", httpContext.Request.Path);
            return true;
        }

        LogException(exception);

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        var response = ExceptionMapper.Map(exception, traceId);

        try
        {
            httpContext.Response.StatusCode = response.Status;
            await httpContext.Response.WriteAsJsonAsync(
                response, options: null, contentType: "application/problem+json", cancellationToken);
        }
        catch (Exception writeException)
        {
            // Response đã bắt đầu gửi / kết nối đứt → không thể ghi thêm; để pipeline mặc định xử lý
            logger.LogWarning(writeException, "Could not write error response for {Path}", httpContext.Request.Path);
            return false;
        }

        return true;
    }

    private void LogException(Exception exception)
    {
        switch (exception)
        {
            // Lỗi do người dùng/nghiệp vụ: chỉ cảnh báo, không cần stack trace
            case DomainException or BusinessRuleValidationException:
                logger.LogWarning("Handled business exception {ExceptionType}: {Message}",
                    exception.GetType().Name, exception.Message);
                break;
            default:
                logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                break;
        }
    }
}
