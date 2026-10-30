using Bms.UnitTests.Fakes;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using static Bms.UnitTests.Persistence.TenantDbContextTests;

namespace Bms.UnitTests.Persistence;

/// <summary>
/// Kiểm tra SQL sinh ra với provider PostgreSQL thật (ToQueryString không cần kết nối DB):
/// filter tenant + soft delete được dịch xuống WHERE và tên cột snake_case.
/// </summary>
public sealed class BmsDbContextSqlTests
{
    private sealed class NpgsqlTestDbContext(DbContextOptions options, FakeTenantContext tenant)
        : TenantDbContextBase(options, tenant, new FakeCompanyContext(null))
    {
        protected override void ConfigureModel(ModelBuilder modelBuilder) => modelBuilder.Entity<Product>();
    }

    [Fact]
    public void Query_TranslatesTenantAndSoftDeleteFiltersToSql()
    {
        var options = new DbContextOptionsBuilder()
            .UseNpgsql("Host=localhost;Database=unit_test")
            .UseSnakeCaseNamingConvention()
            .Options;
        using var db = new NpgsqlTestDbContext(options, new FakeTenantContext(Guid.CreateVersion7()));

        var sql = db.Set<Product>().ToQueryString();

        Assert.Contains("tenant_id", sql);
        Assert.Contains("is_deleted", sql);
        Assert.Contains("WHERE", sql);
    }
}
