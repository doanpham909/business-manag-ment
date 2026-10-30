using Bms.Domain.Common.Rules;
using BuildingBlocks.Contracts.Domain.DDD;
using BuildingBlocks.Contracts.Domain.Entities;
using BuildingBlocks.Contracts.Domain.Exceptions;
using BuildingBlocks.Contracts.Persistence;

namespace Bms.Domain.Admin.Companies;

/// <summary>
/// Công ty (pháp nhân) thuộc tenant — ADR-0002. Dữ liệu nghiệp vụ tách theo công ty.
/// Công ty Ngừng hoạt động không chọn được để làm việc, không nhận dữ liệu mới (DEC-019).
/// </summary>
public sealed class Company : EventEntityAuditBase<Guid>, IAggregateRoot, ITenantEntity, IVersioned
{
    public uint Version { get; private set; }

    public Guid TenantId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? TaxCode { get; private set; }
    public string? Address { get; private set; }
    public string? Phone { get; private set; }
    public CompanyStatus Status { get; private set; }

    private Company() { }

    public static Company Create(string code, string name, string? taxCode, string? address, string? phone,
        Guid tenantId = default)
    {
        CheckRule(new TextMustNotBeEmptyRule(code, nameof(Code), "Mã công ty"));
        var company = new Company { Id = Guid.CreateVersion7(), TenantId = tenantId, Code = code.Trim(), Status = CompanyStatus.Active };
        company.SetInfo(name, taxCode, address, phone);
        return company;
    }

    public void Update(string name, string? taxCode, string? address, string? phone) =>
        SetInfo(name, taxCode, address, phone);

    public void SetStatus(CompanyStatus status) => Status = status;

    private void SetInfo(string name, string? taxCode, string? address, string? phone)
    {
        CheckRule(new TextMustNotBeEmptyRule(name, nameof(Name), "Tên công ty"));
        Name = name.Trim();
        TaxCode = taxCode?.Trim();
        Address = address?.Trim();
        Phone = phone?.Trim();
    }
}

public enum CompanyStatus
{
    Active,
    Inactive,
}

public sealed class CompanyNotFoundException(Guid id)
    : NotFoundException($"Không tìm thấy công ty (Id = {id}).");

public sealed class CompanyCodeAlreadyExistsException(string code)
    : ConflictException($"Mã công ty \"{code}\" đã tồn tại.");
