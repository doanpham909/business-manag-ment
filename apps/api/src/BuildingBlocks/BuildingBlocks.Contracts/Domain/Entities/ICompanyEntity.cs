namespace BuildingBlocks.Contracts.Domain.Entities;

/// <summary>
/// Entity thuộc một công ty trong tenant (ADR-0002). CompanyId do interceptor gán khi insert từ
/// ICompanyContext; global filter "Company" chỉ trả bản ghi của công ty được đọc.
/// </summary>
public interface ICompanyEntity : ITenantEntity
{
    Guid CompanyId { get; }
}
