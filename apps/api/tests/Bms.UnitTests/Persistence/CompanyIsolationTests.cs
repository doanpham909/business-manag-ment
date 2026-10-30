using Bms.UnitTests.Fakes;
using BuildingBlocks.Contracts.Domain.Entities;
using BuildingBlocks.Infrastructure.Auditing;
using BuildingBlocks.Infrastructure.Interceptors;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bms.UnitTests.Persistence;

/// <summary>
/// Cô lập dữ liệu giữa 2 công ty cùng tenant (ADR-0002): không đọc/sửa chéo; chế độ Tất cả công ty
/// đọc gộp được nhưng không ghi được; nhật ký thay đổi được ghi kèm công ty.
/// </summary>
public sealed class CompanyIsolationTests
{
    private static readonly Guid Tenant = Guid.CreateVersion7();
    private static readonly Guid CompanyA = Guid.CreateVersion7();
    private static readonly Guid CompanyB = Guid.CreateVersion7();
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 8, 0, 0, TimeSpan.Zero);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    internal sealed class Voucher : EntityAuditBase<Guid>, ICompanyEntity
    {
        private Voucher() { }

        public Guid TenantId { get; private set; }
        public Guid CompanyId { get; private set; }
        public string Code { get; private set; } = string.Empty;

        public static Voucher Create(string code) => new() { Id = Guid.CreateVersion7(), Code = code };

        public void Rename(string code) => Code = code;
    }

    private sealed class TestDbContext(DbContextOptions options, FakeTenantContext tenant, FakeCompanyContext company)
        : TenantDbContextBase(options, tenant, company)
    {
        protected override void ConfigureModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Voucher>();
            modelBuilder.Entity<AuditLog>();
        }
    }

    private static TestDbContext Create(FakeCompanyContext company, string databaseName)
    {
        var tenant = new FakeTenantContext(Tenant);
        var options = new DbContextOptionsBuilder()
            .UseInMemoryDatabase(databaseName)
            .AddInterceptors(
                new TenantEntityInterceptor(tenant),
                new CompanyEntityInterceptor(company, tenant),
                new AuditLogInterceptor(tenant, company, new FakeCurrentUser(Guid.CreateVersion7()), new FixedTimeProvider(Now)),
                new AuditableEntityInterceptor(new FakeCurrentUser(null), new FixedTimeProvider(Now)))
            .Options;
        return new TestDbContext(options, tenant, company);
    }

    private static async Task SeedAsync(string db)
    {
        await using (var a = Create(new FakeCompanyContext(CompanyA), db))
        {
            a.Add(Voucher.Create("A-1"));
            await a.SaveChangesAsync(Ct);
        }

        await using var b = Create(new FakeCompanyContext(CompanyB), db);
        b.Add(Voucher.Create("B-1"));
        await b.SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task Query_ReturnsOnlyActiveCompanyRows()
    {
        var db = Guid.NewGuid().ToString();
        await SeedAsync(db);

        await using var a = Create(new FakeCompanyContext(CompanyA), db);
        var codes = await a.Set<Voucher>().Select(v => v.Code).ToListAsync(Ct);

        Assert.Equal(["A-1"], codes);
    }

    [Fact]
    public async Task Query_WithoutSelectedCompany_ReturnsNothing()
    {
        var db = Guid.NewGuid().ToString();
        await SeedAsync(db);

        await using var none = Create(new FakeCompanyContext(null), db);

        Assert.Empty(await none.Set<Voucher>().ToListAsync(Ct));
    }

    [Fact]
    public async Task AllCompaniesMode_ReadsBothCompanies_ButCannotWrite()
    {
        var db = Guid.NewGuid().ToString();
        await SeedAsync(db);

        await using var all = Create(new FakeCompanyContext(null, [CompanyA, CompanyB]), db);
        Assert.Equal(2, await all.Set<Voucher>().CountAsync(Ct));

        all.Add(Voucher.Create("X"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => all.SaveChangesAsync(Ct));
    }

    [Fact]
    public async Task Update_RowOfAnotherCompany_IsBlocked()
    {
        var db = Guid.NewGuid().ToString();
        await SeedAsync(db);

        var company = new FakeCompanyContext(CompanyB);
        await using var ctx = Create(company, db);
        var row = await ctx.Set<Voucher>().SingleAsync(Ct);
        row.Rename("hacked");
        company.ActiveCompanyId = CompanyA; // đổi công ty giữa chừng → dòng không thuộc công ty đang làm việc

        await Assert.ThrowsAsync<InvalidOperationException>(() => ctx.SaveChangesAsync(Ct));
    }

    [Fact]
    public async Task Insert_StampsCompanyAndWritesAuditLog()
    {
        var db = Guid.NewGuid().ToString();
        await using var a = Create(new FakeCompanyContext(CompanyA), db);
        a.Add(Voucher.Create("A-2"));
        await a.SaveChangesAsync(Ct);

        var voucher = await a.Set<Voucher>().SingleAsync(Ct);
        var log = await a.Set<AuditLog>().SingleAsync(Ct);
        Assert.Equal(CompanyA, voucher.CompanyId);
        Assert.Equal((AuditAction.Created, CompanyA, nameof(Voucher)), (log.Action, log.CompanyId, log.EntityType));
        Assert.Contains("A-2", log.Changes);
    }
}
