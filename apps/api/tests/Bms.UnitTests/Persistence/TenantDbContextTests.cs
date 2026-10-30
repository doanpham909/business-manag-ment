using Bms.UnitTests.Fakes;
using BuildingBlocks.Contracts.Domain.Entities;
using BuildingBlocks.Contracts.Persistence;
using BuildingBlocks.Infrastructure.Interceptors;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bms.UnitTests.Persistence;

/// <summary>Kiểm tra interceptor (audit, tenant, soft delete) + global query filter trên DbContext base.</summary>
public sealed class TenantDbContextTests
{
    private static readonly Guid TenantA = Guid.CreateVersion7();
    private static readonly Guid TenantB = Guid.CreateVersion7();
    private static readonly Guid UserId = Guid.CreateVersion7();
    private static readonly DateTimeOffset Now = new(2026, 10, 29, 1, 2, 3, TimeSpan.Zero);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    internal sealed class Product : EntityAuditBase<Guid>, ITenantEntity
    {
        private Product() { }

        public Guid TenantId { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public static Product Create(string name) => new() { Id = Guid.CreateVersion7(), Name = name };

        /// <summary>Giả lập entity do client "dựng" từ request (Id + TenantId tùy ý, không load từ DB).</summary>
        public static Product Forge(Guid id, Guid tenantId, string name) => new() { Id = id, TenantId = tenantId, Name = name };
    }

    /// <summary>Tenant B tạo 1 dòng; trả về Id để tenant A thử tấn công.</summary>
    private static async Task<Guid> SeedTenantBRowAsync(string databaseName)
    {
        await using var db = CreateContext(new FakeTenantContext(TenantB), databaseName);
        var product = Product.Create("B-row");
        db.Add(product);
        await db.SaveChangesAsync(Ct);
        return product.Id;
    }

    private static async Task<Product> LoadRawAsync(string databaseName, Guid id)
    {
        await using var db = CreateContext(new FakeTenantContext(null), databaseName);
        return await db.Set<Product>().IgnoreQueryFilters().AsNoTracking().SingleAsync(p => p.Id == id, Ct);
    }

    [Theory]
    [InlineData(true)]   // dựng entity với TenantId của nạn nhân
    [InlineData(false)]  // dựng entity với TenantId rỗng
    public async Task CrossTenant_UpdateOrRemoveOfForgedEntity_Throws(bool useVictimTenant)
    {
        var dbName = Guid.NewGuid().ToString();
        var victimId = await SeedTenantBRowAsync(dbName);
        var forgedTenant = useVictimTenant ? TenantB : Guid.Empty;

        await using (var db = CreateContext(new FakeTenantContext(TenantA), dbName))
        {
            db.Update(Product.Forge(victimId, forgedTenant, "HACKED"));
            await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync(Ct));
        }

        await using (var db = CreateContext(new FakeTenantContext(TenantA), dbName))
        {
            db.Remove(Product.Forge(victimId, forgedTenant, ""));
            await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync(Ct));
        }

        var row = await LoadRawAsync(dbName, victimId);
        Assert.Equal(("B-row", false, TenantB), (row.Name, row.IsDeleted, row.TenantId));
    }

    [Fact]
    public async Task Repository_UpdateOrRemoveOfDetachedEntity_Throws()
    {
        await using var db = CreateContext(new FakeTenantContext(TenantA), Guid.NewGuid().ToString());
        var repository = new RepositoryBase<Product, Guid, TestDbContext>(db);
        var detached = Product.Forge(Guid.CreateVersion7(), TenantA, "x");

        Assert.Throws<InvalidOperationException>(() => repository.Update(detached));
        Assert.Throws<InvalidOperationException>(() => repository.Remove(detached));
    }

    [Fact]
    public async Task SoftDelete_OnlyWritesDeleteColumns()
    {
        var dbName = Guid.NewGuid().ToString();
        await using (var db = CreateContext(new FakeTenantContext(TenantA), dbName))
        {
            var product = Product.Create("Laptop");
            db.Add(product);
            await db.SaveChangesAsync(Ct);
            db.ChangeTracker.Clear();

            // Stub chỉ có Id (Name rỗng) — xóa mềm không được ghi đè Name
            db.Remove(Product.Forge(product.Id, TenantA, ""));
            await db.SaveChangesAsync(Ct);

            var row = await LoadRawAsync(dbName, product.Id);
            Assert.Equal(("Laptop", true, Now), (row.Name, row.IsDeleted, row.DeletedAt));
        }
    }

    private sealed class TestDbContext(DbContextOptions options, FakeTenantContext tenant)
        : TenantDbContextBase(options, tenant, new FakeCompanyContext(null))
    {
        protected override void ConfigureModel(ModelBuilder modelBuilder) => modelBuilder.Entity<Product>();
    }

    private static TestDbContext CreateContext(FakeTenantContext tenant, string databaseName)
    {
        var options = new DbContextOptionsBuilder()
            .UseInMemoryDatabase(databaseName)
            .AddInterceptors(
                new TenantEntityInterceptor(tenant),
                new AuditableEntityInterceptor(new FakeCurrentUser(UserId), new FixedTimeProvider(Now)))
            .Options;
        return new TestDbContext(options, tenant);
    }

    [Fact]
    public async Task Insert_StampsTenantAndAudit()
    {
        var tenant = new FakeTenantContext(TenantA);
        await using var db = CreateContext(tenant, Guid.NewGuid().ToString());
        var product = Product.Create("Laptop");

        db.Add(product);
        await db.SaveChangesAsync(Ct);

        Assert.Equal(TenantA, product.TenantId);
        Assert.Equal(Now, product.CreatedAt);
        Assert.Equal(UserId, product.CreatedBy);
    }

    [Fact]
    public async Task Insert_WithoutTenant_Throws()
    {
        await using var db = CreateContext(new FakeTenantContext(null), Guid.NewGuid().ToString());
        db.Add(Product.Create("Laptop"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync(Ct));
    }

    [Fact]
    public async Task Query_OnlyReturnsCurrentTenantRows()
    {
        var tenant = new FakeTenantContext(TenantA);
        await using var db = CreateContext(tenant, Guid.NewGuid().ToString());
        db.Add(Product.Create("A1"));
        await db.SaveChangesAsync(Ct);
        tenant.TenantId = TenantB;
        db.Add(Product.Create("B1"));
        await db.SaveChangesAsync(Ct);

        // Filter tham số hóa theo CurrentTenantId → đổi tenant là đổi kết quả
        Assert.Equal(["B1"], await db.Set<Product>().Select(p => p.Name).ToListAsync(Ct));
        tenant.TenantId = null;
        Assert.Empty(await db.Set<Product>().ToListAsync(Ct));

        var all = await db.Set<Product>().IgnoreQueryFilters([QueryFilterNames.Tenant]).CountAsync(Ct);
        Assert.Equal(2, all);
    }

    [Fact]
    public async Task Remove_SoftDeletesAndHidesRow()
    {
        var tenant = new FakeTenantContext(TenantA);
        await using var db = CreateContext(tenant, Guid.NewGuid().ToString());
        var product = Product.Create("Laptop");
        db.Add(product);
        await db.SaveChangesAsync(Ct);

        db.Remove(product);
        await db.SaveChangesAsync(Ct);

        Assert.True(product.IsDeleted);
        Assert.Equal(Now, product.DeletedAt);
        Assert.Equal(UserId, product.DeletedBy);
        Assert.Empty(await db.Set<Product>().ToListAsync(Ct));
        Assert.Single(await db.Set<Product>().IgnoreQueryFilters([QueryFilterNames.SoftDelete]).ToListAsync(Ct));
    }

    [Fact]
    public async Task Update_StampsUpdatedAndForbidsTenantChange()
    {
        var tenant = new FakeTenantContext(TenantA);
        await using var db = CreateContext(tenant, Guid.NewGuid().ToString());
        var product = Product.Create("Laptop");
        db.Add(product);
        await db.SaveChangesAsync(Ct);

        db.Entry(product).Property(p => p.Name).CurrentValue = "Laptop Pro";
        await db.SaveChangesAsync(Ct);
        Assert.Equal(Now, product.UpdatedAt);

        db.Entry(product).Property(p => p.TenantId).CurrentValue = TenantB;
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync(Ct));
    }
}
