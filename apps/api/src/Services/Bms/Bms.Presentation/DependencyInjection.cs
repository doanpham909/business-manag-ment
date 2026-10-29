using System.Text.Encodings.Web;
using System.Text.Unicode;
using BuildingBlocks.Shared;
using BuildingBlocks.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Bms.Presentation;

/// <summary>Đăng ký controller của Presentation + cấu hình JSON/model validation.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddApplicationPart(typeof(DependencyInjection).Assembly)
            .AddJsonOptions(options =>
                // Giữ nguyên tiếng Việt có dấu trong JSON (không escape \uXXXX) — giống QMS
                options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All))
            .ConfigureApiBehaviorOptions(options =>
                options.InvalidModelStateResponseFactory = CreateModelStateErrorResponse);

        return services;
    }

    /// <summary>
    /// Lỗi binding/model state (vd: sai kiểu dữ liệu) → 400 cùng shape ErrorResponse
    /// thay vì ValidationProblemDetails mặc định, để FE chỉ xử lý MỘT định dạng lỗi.
    /// </summary>
    private static BadRequestObjectResult CreateModelStateErrorResponse(ActionContext context)
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error => new ErrorDetail(
                entry.Key,
                string.IsNullOrWhiteSpace(error.ErrorMessage) ? "The value is invalid." : error.ErrorMessage)))
            .ToArray();

        var response = new ErrorResponse(
            ErrorCodes.Validation, "ValidationError", "Validation Error", StatusCodes.Status400BadRequest,
            "A validation problem occurred.", errors,
            System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);

        return new BadRequestObjectResult(response);
    }
}
