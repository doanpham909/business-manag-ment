using BuildingBlocks.Infrastructure.Behaviors;
using BuildingBlocks.Infrastructure.Exceptions;
using BuildingBlocks.Infrastructure.Interceptors;
using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure;

/// <summary>Đăng ký DI cho các thành phần dùng chung của BuildingBlocks.Infrastructure.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký MediatR pipeline behaviors. THỨ TỰ ĐĂNG KÝ = THỨ TỰ CHẠY (ngoài → trong):
    /// Logging → Validation → Transaction → Handler.
    /// </summary>
    public static IServiceCollection AddPipelineBehaviors(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestLoggingPipelineBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionPipelineBehavior<,>));
        return services;
    }

    /// <summary>
    /// Đăng ký EF SaveChanges interceptors (scoped vì phụ thuộc user/tenant của request).
    /// DbContext lấy chúng qua <c>sp.GetServices&lt;ISaveChangesInterceptor&gt;()</c>.
    /// </summary>
    public static IServiceCollection AddEfInterceptors(this IServiceCollection services)
    {
        services.AddScoped<ISaveChangesInterceptor, TenantEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, PublishDomainEventsInterceptor>();
        return services;
    }

    /// <summary>Đăng ký global exception handler + ProblemDetails.</summary>
    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }
}
