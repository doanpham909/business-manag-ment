using Bms.Domain.Common.Rules;
using BuildingBlocks.Contracts.Domain.DDD;
using BuildingBlocks.Contracts.Domain.Entities;
using BuildingBlocks.Contracts.Domain.Exceptions;
using BuildingBlocks.Contracts.Persistence;

namespace Bms.Domain.Admin.Catalogs;

/// <summary>
/// Một mục trong danh mục dùng chung cho cả tập đoàn (ADM-05, cấp tenant — DEC-008). Loại danh mục xem
/// <see cref="CatalogTypes"/>. Mục ngừng sử dụng không chọn được cho dữ liệu mới, vẫn hiển thị trên chứng từ cũ.
/// </summary>
public sealed class CatalogItem : EventEntityAuditBase<Guid>, IAggregateRoot, ITenantEntity, IVersioned
{
    public uint Version { get; private set; }
    public Guid TenantId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    private CatalogItem() { }

    public static CatalogItem Create(string type, string code, string name, string? description, int sortOrder,
        Guid tenantId = default)
    {
        CheckRule(new TextMustNotBeEmptyRule(code, nameof(Code), "Mã"));
        var item = new CatalogItem
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Type = type,
            Code = code.Trim(),
            IsActive = true,
        };
        item.Update(name, description, sortOrder);
        return item;
    }

    public void Update(string name, string? description, int sortOrder)
    {
        CheckRule(new TextMustNotBeEmptyRule(name, nameof(Name), "Tên"));
        Name = name.Trim();
        Description = description?.Trim();
        SortOrder = sortOrder;
    }

    public void SetActive(bool isActive) => IsActive = isActive;
}

/// <summary>Các loại danh mục dùng chung.</summary>
public static class CatalogTypes
{
    public const string Unit = "unit";
    public const string ProjectType = "project-type";
    public const string ConstructionType = "construction-type";
    public const string DepartmentType = "department-type";
    public const string CostGroup = "cost-group";
    public const string MaterialGroup = "material-group";
    public const string Bank = "bank";

    public static readonly IReadOnlyDictionary<string, string> Names = new Dictionary<string, string>
    {
        [Unit] = "Đơn vị tính",
        [ProjectType] = "Loại dự án",
        [ConstructionType] = "Loại công trình",
        [DepartmentType] = "Loại đơn vị",
        [CostGroup] = "Nhóm chi phí",
        [MaterialGroup] = "Nhóm vật tư",
        [Bank] = "Ngân hàng",
    };
}

public sealed class CatalogItemNotFoundException(Guid id) : NotFoundException($"Không tìm thấy mục danh mục (Id = {id}).");

public sealed class CatalogCodeAlreadyExistsException(string code)
    : ConflictException($"Mã \"{code}\" đã tồn tại trong danh mục này.");

public sealed class CatalogTypeInvalidException(string type) : BadRequestException($"Loại danh mục \"{type}\" không hợp lệ.");
