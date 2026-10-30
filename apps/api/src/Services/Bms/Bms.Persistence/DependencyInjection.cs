using Bms.Persistence.Repositories;
using BuildingBlocks.Contracts.Persistence;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bms.Persistence;

/// <summary>Đăng ký DI cho tầng Persistence.</summary>
public static class DependencyInjection
{
    /// <summary>Tên connection string trong cấu hình (ConnectionStrings:Default).</summary>
    public const string ConnectionStringName = "Default";

    /// <summary>
    /// Đăng ký BmsDbContext (Npgsql + snake_case + interceptors), UnitOfWork, repository generic
    /// và health check DB. Connection string chứa mật khẩu phải đặt qua user-secrets / biến môi trường,
    /// KHÔNG commit vào appsettings.
    /// </summary>
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"Missing connection string 'ConnectionStrings:{ConnectionStringName}'. " +
                "Set it via 'dotnet user-secrets' (dev) or environment variable 'ConnectionStrings__Default'.");

        services.AddEfInterceptors();

        services.AddDbContext<BmsDbContext>((serviceProvider, options) => options
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history"))
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>()));

        services.AddScoped<IUnitOfWork, EfUnitOfWork<BmsDbContext>>();
        services.AddScoped(typeof(IRepositoryBase<,>), typeof(BmsRepository<,>));
        services.AddScoped<IDocumentNumberGenerator, Numbering.DocumentNumberGenerator>();

        // DB lỗi/không kết nối được → báo Unhealthy ở /health, không làm sập app
        services.AddHealthChecks().AddDbContextCheck<BmsDbContext>("database", tags: ["db"]);

        return services;
    }
}
