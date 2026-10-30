using Bms.Domain.Common.Rules;
using BuildingBlocks.Contracts.Domain.DDD;
using BuildingBlocks.Contracts.Domain.Entities;
using BuildingBlocks.Contracts.Domain.Exceptions;
using BuildingBlocks.Contracts.Persistence;

namespace Bms.Domain.Hrm.Departments;

/// <summary>
/// Đơn vị trong cơ cấu tổ chức của một công ty (HRM-01). Path là chuỗi Id từ gốc tới đơn vị
/// ("/id-gốc/id-con/"), dùng để lọc phạm vi "đơn vị và cấp dưới" không cần truy vấn đệ quy.
/// </summary>
public sealed class Department : EventEntityAuditBase<Guid>, IAggregateRoot, ICompanyEntity, IVersioned
{
    public uint Version { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CompanyId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid? ParentId { get; private set; }
    public string Path { get; private set; } = string.Empty;

    /// <summary>Mã mục danh mục "Loại đơn vị" (Phòng, Ban, Đội, Công trường…).</summary>
    public string? TypeCode { get; private set; }

    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    private Department() { }

    public static Department Create(string code, string name, Department? parent, string? typeCode, int sortOrder)
    {
        CheckRule(new TextMustNotBeEmptyRule(code, nameof(Code), "Mã đơn vị"));
        var department = new Department { Id = Guid.CreateVersion7(), Code = code.Trim(), IsActive = true };
        department.Update(name, typeCode, sortOrder);
        department.MoveTo(parent);
        return department;
    }

    public void Update(string name, string? typeCode, int sortOrder)
    {
        CheckRule(new TextMustNotBeEmptyRule(name, nameof(Name), "Tên đơn vị"));
        Name = name.Trim();
        TypeCode = typeCode?.Trim();
        SortOrder = sortOrder;
    }

    /// <summary>Đổi đơn vị cha. Không được chuyển vào chính nó hoặc đơn vị cấp dưới của nó.</summary>
    public void MoveTo(Department? parent)
    {
        CheckRule(new DepartmentMustNotMoveUnderItselfRule(this, parent));
        ParentId = parent?.Id;
        Path = $"{parent?.Path ?? "/"}{Id}/";
    }

    public void SetActive(bool isActive) => IsActive = isActive;
}

public sealed class DepartmentMustNotMoveUnderItselfRule(Department department, Department? parent) : IBusinessRule
{
    public bool IsBroken() => parent is not null && parent.Path.Contains($"/{department.Id}/", StringComparison.Ordinal);

    public string Message => "Không thể chuyển đơn vị vào chính nó hoặc đơn vị cấp dưới của nó.";

    public string FieldName => nameof(Department.ParentId);
}

public sealed class DepartmentNotFoundException(Guid id) : NotFoundException($"Không tìm thấy đơn vị (Id = {id}).");

public sealed class DepartmentCodeAlreadyExistsException(string code)
    : ConflictException($"Mã đơn vị \"{code}\" đã tồn tại trong công ty.");

public sealed class DepartmentHasChildrenException()
    : ConflictException("Đơn vị còn đơn vị cấp dưới, không xóa được.");
