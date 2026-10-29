using BuildingBlocks.Contracts.Abstractions;
using BuildingBlocks.Contracts.Persistence;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Bms.UnitTests.Fakes;

/// <summary>TimeProvider trả về thời điểm cố định.</summary>
internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;
}

/// <summary>Tenant context có thể đổi trong test.</summary>
internal sealed class FakeTenantContext(Guid? tenantId) : ITenantContext
{
    public Guid? TenantId { get; set; } = tenantId;
}

/// <summary>User hiện tại giả lập.</summary>
internal sealed class FakeCurrentUser(Guid? userId) : ICurrentUser
{
    public Guid? UserId { get; } = userId;

    public bool IsAuthenticated => UserId.HasValue;
}

/// <summary>UnitOfWork đếm số lần SaveChanges.</summary>
internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public int DiscardCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(0);
    }

    public void DiscardChanges() => DiscardCount++;
}

/// <summary>IHostEnvironment tối giản.</summary>
internal sealed class FakeHostEnvironment(string environmentName) : IHostEnvironment
{
    public string EnvironmentName { get; set; } = environmentName;

    public string ApplicationName { get; set; } = "Bms.API";

    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
