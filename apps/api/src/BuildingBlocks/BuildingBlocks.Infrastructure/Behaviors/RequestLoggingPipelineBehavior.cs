using System.Diagnostics;
using BuildingBlocks.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Behaviors;

/// <summary>
/// Behavior ngoài cùng: log tên request, thời gian xử lý, cảnh báo khi chậm hoặc Result thất bại.
/// Gộp RequestLogging + Performance behavior của QMS cho gọn. Exception được để nổi lên
/// GlobalExceptionHandler (nơi log đầy đủ stack trace) — tránh log trùng.
/// </summary>
public sealed class RequestLoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>Ngưỡng coi là request chậm.</summary>
    internal const int SlowRequestThresholdMs = 3000;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var startedAt = Stopwatch.GetTimestamp();

        logger.LogInformation("Processing request {RequestName}", requestName);

        var response = await next(cancellationToken);

        var elapsedMs = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;

        if (response is Result { IsFailure: true } failure)
        {
            logger.LogWarning(
                "Request {RequestName} failed with {ErrorCode} in {ElapsedMs:0.0} ms",
                requestName, failure.Error.Code, elapsedMs);
        }
        else if (elapsedMs > SlowRequestThresholdMs)
        {
            // Không log toàn bộ payload request để tránh lộ dữ liệu nhạy cảm
            logger.LogWarning("Long running request {RequestName} took {ElapsedMs:0.0} ms", requestName, elapsedMs);
        }
        else
        {
            logger.LogInformation("Request {RequestName} processed in {ElapsedMs:0.0} ms", requestName, elapsedMs);
        }

        return response;
    }
}
