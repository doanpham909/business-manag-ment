using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Infrastructure.HealthChecks;

/// <summary>Map endpoint health trả JSON (giống QMS HealthChecksExtensions).</summary>
public static class HealthCheckEndpointExtensions
{
    /// <summary>
    /// Map <paramref name="path"/> → JSON { status, totalDuration, entries[] }.
    /// Healthy → 200, Unhealthy → 503. Check lỗi (vd: DB không kết nối được) KHÔNG làm sập app.
    /// </summary>
    public static IEndpointConventionBuilder MapAppHealthEndpoint(
        this IEndpointRouteBuilder endpoints,
        string path = "/health")
        => endpoints.MapHealthChecks(path, new HealthCheckOptions { ResponseWriter = WriteJsonAsync });

    private static Task WriteJsonAsync(HttpContext context, HealthReport report)
    {
        var payload = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                // Chỉ trả loại exception, không trả message (có thể chứa host/user của connection string)
                error = entry.Value.Exception?.GetType().Name,
                tags = entry.Value.Tags,
            }),
        };

        return context.Response.WriteAsJsonAsync(payload, options: null, contentType: "application/json; charset=utf-8");
    }
}
