using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Bms.Application;

/// <summary>Đánh dấu assembly Application (dùng để scan handler/validator, test kiến trúc).</summary>
public static class AssemblyReference
{
    public static readonly System.Reflection.Assembly Assembly = typeof(AssemblyReference).Assembly;
}

/// <summary>Đăng ký DI cho tầng Application.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký MediatR handler + FluentValidation validator trong assembly này.
    /// Pipeline behaviors (logging/validation/transaction) đăng ký ở API qua AddPipelineBehaviors().
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly));
        services.AddValidatorsFromAssembly(AssemblyReference.Assembly, includeInternalTypes: true);
        return services;
    }
}
