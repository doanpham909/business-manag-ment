namespace Bms.API.Extensions;

/// <summary>CORS: chỉ cho phép các origin cấu hình trong "Cors:AllowedOrigins".</summary>
public static class CorsExtensions
{
    public const string PolicyName = "BmsCors";

    /// <summary>
    /// Development cấu hình http://localhost:4200 (appsettings.Development.json).
    /// Không cấu hình origin nào → không cho phép cross-origin (an toàn mặc định).
    /// </summary>
    public static IServiceCollection AddApiCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options => options.AddPolicy(PolicyName, policy =>
        {
            if (origins.Length > 0)
                policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
        }));

        return services;
    }
}
