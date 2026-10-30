using System.Security.Cryptography;
using Bms.Infrastructure.Identity;
using Bms.Infrastructure.Security;
using BuildingBlocks.Contracts.Abstractions;
using BuildingBlocks.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Bms.Infrastructure;

/// <summary>Đăng ký DI cho tầng Infrastructure.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddHttpContextAccessor();
        services.TryAddSingleton(TimeProvider.System);

        // Scoped: mỗi request một user/tenant/công ty
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<ICompanyContext, CompanyContext>();

        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();

        AddJwtAuthentication(services, configuration, environment);
        return services;
    }

    private static void AddJwtAuthentication(
        IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwt.SigningKey))
        {
            // Dev: khóa ngẫu nhiên theo tiến trình (token mất hiệu lực khi khởi động lại). Môi trường khác bắt buộc cấu hình.
            if (!environment.IsDevelopment())
                throw new InvalidOperationException("Missing 'Jwt:SigningKey'. Set it via user-secrets or environment variable.");
            jwt.SigningKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        }

        services.Configure<JwtOptions>(options =>
        {
            options.Issuer = jwt.Issuer;
            options.Audience = jwt.Audience;
            options.SigningKey = jwt.SigningKey;
            options.AccessTokenMinutes = jwt.AccessTokenMinutes;
            options.RefreshTokenDays = jwt.RefreshTokenDays;
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false; // giữ nguyên tên claim: sub, tenant_id, perm...
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = JwtAccessTokenIssuer.SigningKey(jwt),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = ClaimNames.Name,
                };
            });

        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
    }
}
