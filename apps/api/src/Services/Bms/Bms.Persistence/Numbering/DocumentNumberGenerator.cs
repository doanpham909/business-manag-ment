using Bms.Persistence.Constants;
using BuildingBlocks.Contracts.Abstractions;
using BuildingBlocks.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bms.Persistence.Numbering;

/// <summary>Bộ đếm số chứng từ theo (tenant, công ty, loại, kỳ yyyyMM).</summary>
internal sealed class DocumentSequence
{
    public Guid TenantId { get; set; }
    public Guid CompanyId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public long LastNumber { get; set; }
}

internal sealed class DocumentSequenceConfiguration : IEntityTypeConfiguration<DocumentSequence>
{
    public void Configure(EntityTypeBuilder<DocumentSequence> builder)
    {
        builder.ToTable(TableNames.DocumentSequences, TableNames.SystemSchema);
        builder.HasKey(x => new { x.TenantId, x.CompanyId, x.DocumentType, x.Period });
        builder.Property(x => x.DocumentType).HasMaxLength(50);
        builder.Property(x => x.Period).HasMaxLength(6);
    }
}

/// <summary>
/// Cấp số bằng một câu INSERT ... ON CONFLICT DO UPDATE ... RETURNING: PostgreSQL khóa dòng bộ đếm nên
/// hai người lập chứng từ cùng lúc không bao giờ nhận cùng một số. SQL thô → tự lọc tenant/công ty.
/// </summary>
internal sealed class DocumentNumberGenerator(BmsDbContext db, ITenantContext tenant, ICompanyContext company)
    : IDocumentNumberGenerator
{
    public async Task<string> NextAsync(
        string documentType, string prefix, DateOnly documentDate, CancellationToken cancellationToken)
    {
        var tenantId = tenant.TenantId ?? throw new InvalidOperationException("No tenant in context.");
        var companyId = company.ActiveCompanyId ?? throw new InvalidOperationException("No active company in context.");
        var period = documentDate.ToString("yyyyMM", System.Globalization.CultureInfo.InvariantCulture);

        var numbers = await db.Database.SqlQuery<long>($"""
            INSERT INTO system.document_sequences (tenant_id, company_id, document_type, period, last_number)
            VALUES ({tenantId}, {companyId}, {documentType}, {period}, 1)
            ON CONFLICT (tenant_id, company_id, document_type, period)
            DO UPDATE SET last_number = system.document_sequences.last_number + 1
            RETURNING last_number AS "Value"
            """).ToListAsync(cancellationToken);

        return $"{prefix}-{period}-{numbers[0]:0000}";
    }
}
