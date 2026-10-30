using Bms.Domain.Common.Rules;
using BuildingBlocks.Contracts.Domain.DDD;
using BuildingBlocks.Contracts.Domain.Entities;
using BuildingBlocks.Contracts.Domain.Exceptions;
using BuildingBlocks.Contracts.Persistence;

namespace Bms.Domain.Projects;

/// <summary>
/// Dự án của một công ty (PRJ-01). Có thể có dự án con / gói thầu (DEC-021): dự án con là một dự án đầy đủ
/// có chọn dự án cha cùng công ty. Dự án Hoàn thành / Đóng không chọn được khi lập chứng từ mới (DEC-018, DEC-019).
/// </summary>
public sealed class Project : EventEntityAuditBase<Guid>, IAggregateRoot, ICompanyEntity, IVersioned
{
    public uint Version { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CompanyId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid? ParentProjectId { get; private set; }
    public string? ProjectTypeCode { get; private set; }
    public string? ConstructionTypeCode { get; private set; }
    public string? Location { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public string? ManagerName { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public string? Description { get; private set; }
    public ProjectStatus Status { get; private set; }

    /// <summary>Có được chọn khi lập chứng từ mới hay không.</summary>
    public bool AcceptsNewDocuments => Status == ProjectStatus.InProgress;

    private Project() { }

    public static Project Create(string code, ProjectInfo info)
    {
        CheckRule(new TextMustNotBeEmptyRule(code, nameof(Code), "Mã dự án"));
        var project = new Project { Id = Guid.CreateVersion7(), Code = code.Trim(), Status = ProjectStatus.InProgress };
        project.Update(info);
        return project;
    }

    /// <summary>Sao chép: toàn bộ thông tin dự án trừ mã (DEC-021); dự án mới ở trạng thái Đang thực hiện.</summary>
    public Project Copy(string newCode) => Create(newCode, new ProjectInfo(
        Name, ParentProjectId, ProjectTypeCode, ConstructionTypeCode, Location, DepartmentId, ManagerName,
        StartDate, EndDate, Description));

    public void Update(ProjectInfo info)
    {
        CheckRule(new TextMustNotBeEmptyRule(info.Name, nameof(Name), "Tên dự án"));
        CheckRule(new EndDateMustNotBeBeforeStartDateRule(info.StartDate, info.EndDate, nameof(EndDate)));
        CheckRule(new ProjectMustNotBeItsOwnParentRule(Id, info.ParentProjectId));

        Name = info.Name.Trim();
        ParentProjectId = info.ParentProjectId;
        ProjectTypeCode = info.ProjectTypeCode?.Trim();
        ConstructionTypeCode = info.ConstructionTypeCode?.Trim();
        Location = info.Location?.Trim();
        DepartmentId = info.DepartmentId;
        ManagerName = info.ManagerName?.Trim();
        StartDate = info.StartDate;
        EndDate = info.EndDate;
        Description = info.Description?.Trim();
    }

    public void ChangeStatus(ProjectStatus status)
    {
        CheckRule(new ProjectStatusTransitionMustBeValidRule(Status, status));
        Status = status;
    }
}

/// <summary>Thông tin chung của dự án (mọi trường trừ mã và trạng thái).</summary>
public sealed record ProjectInfo(
    string Name, Guid? ParentProjectId, string? ProjectTypeCode, string? ConstructionTypeCode, string? Location,
    Guid? DepartmentId, string? ManagerName, DateOnly? StartDate, DateOnly? EndDate, string? Description);

public enum ProjectStatus
{
    InProgress,
    Completed,
    Closed,
}

public sealed class ProjectMustNotBeItsOwnParentRule(Guid id, Guid? parentId) : IBusinessRule
{
    public bool IsBroken() => parentId == id;

    public string Message => "Dự án không thể là dự án cha của chính nó.";

    public string FieldName => nameof(Project.ParentProjectId);
}

/// <summary>Đang thực hiện ↔ Hoàn thành ↔ Đóng; không nhảy thẳng Đang thực hiện ↔ Đóng.</summary>
public sealed class ProjectStatusTransitionMustBeValidRule(ProjectStatus from, ProjectStatus to) : IBusinessRule
{
    public bool IsBroken() => (from, to) is (ProjectStatus.InProgress, ProjectStatus.Closed) or (ProjectStatus.Closed, ProjectStatus.InProgress);

    public string Message => "Dự án phải qua trạng thái Hoàn thành trước khi Đóng hoặc mở lại.";

    public string FieldName => nameof(Project.Status);
}

public sealed class ProjectNotFoundException(Guid id) : NotFoundException($"Không tìm thấy dự án (Id = {id}).");

public sealed class ProjectCodeAlreadyExistsException(string code)
    : ConflictException($"Mã dự án \"{code}\" đã tồn tại trong công ty.");

public sealed class ProjectHasChildrenException()
    : ConflictException("Dự án còn dự án con / gói thầu, không xóa được.");
