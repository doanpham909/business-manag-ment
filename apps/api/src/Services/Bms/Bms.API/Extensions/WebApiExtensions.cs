using System.Text.Encodings.Web;
using System.Text.Unicode;
using BuildingBlocks.Shared;
using Microsoft.OpenApi;

namespace Bms.API.Extensions;

/// <summary>Cấu hình web host: Swagger, JSON mặc định, security headers.</summary>
public static class WebApiExtensions
{
    /// <summary>Swagger (Swashbuckle) — chỉ bật UI ở Development (xem Program.cs).</summary>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = AppConstants.AppName,
            Version = "v1",
        }));
        return services;
    }

    /// <summary>
    /// JSON cho các response ngoài MVC (health check, global exception handler):
    /// camelCase (mặc định web) + giữ nguyên tiếng Việt có dấu.
    /// </summary>
    public static IServiceCollection AddApiJsonDefaults(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All));
        return services;
    }

    /// <summary>Header bảo mật cơ bản cho mọi response (API thuần JSON).</summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        => app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            await next(context);
        });
}
