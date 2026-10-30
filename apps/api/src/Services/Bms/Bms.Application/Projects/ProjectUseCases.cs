using Bms.Application.Common.Paging;
using Bms.Domain.Projects;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Contracts.Persistence;
using BuildingBlocks.Shared.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Bms.Application.Projects;

public static class ProjectPermissions
{
    public const string View = "projects.project.view";
    public const string Create = "projects.project.create";
    public const string Update = "projects.project.update";
    public const string Delete = "projects.project.delete";
}

public sealed record ProjectResponse(
    Guid Id, string Code, string Name, Guid? ParentProjectId, string? ProjectTypeCode, string? ConstructionTypeCode,
    string? Location, Guid? DepartmentId, string? ManagerName, DateOnly? StartDate, DateOnly? EndDate,
    string? Description, ProjectStatus Status, uint Version);

public sealed record GetProjectsQuery(
    int PageIndex = 1, int PageSize = 20, string? SearchTerm = null, ProjectStatus? Status = null, bool SelectableOnly = false)
    : IQuery<PagedResult<ProjectResponse>>;

public sealed record GetProjectQuery(Guid Id) : IQuery<ProjectResponse>;

public sealed record CreateProjectCommand(
    string Code, string Name, Guid? ParentProjectId, string? ProjectTypeCode, string? ConstructionTypeCode,
    string? Location, Guid? DepartmentId, string? ManagerName, DateOnly? StartDate, DateOnly? EndDate, string? Description)
    : ICommand<Guid>;

public sealed record UpdateProjectCommand(
    Guid Id, string Name, Guid? ParentProjectId, string? ProjectTypeCode, string? ConstructionTypeCode,
    string? Location, Guid? DepartmentId, string? ManagerName, DateOnly? StartDate, DateOnly? EndDate,
    string? Description, uint? Version) : ICommand;

public sealed record CopyProjectCommand(Guid Id, string NewCode) : ICommand<Guid>;

public sealed record ChangeProjectStatusCommand(Guid Id, string Status, uint? Version) : ICommand;

public sealed record DeleteProjectCommand(Guid Id) : ICommand;

internal sealed class ProjectQueryHandlers(IRepositoryBase<Project, Guid> projects)
    : IQueryHandler<GetProjectsQuery, PagedResult<ProjectResponse>>, IQueryHandler<GetProjectQuery, ProjectResponse>
{
    public async Task<Result<PagedResult<ProjectResponse>>> Handle(GetProjectsQuery request, CancellationToken ct)
    {
        var query = projects.FindAll();
        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status);
        if (request.SelectableOnly)
            query = query.Where(p => p.Status == ProjectStatus.InProgress);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = $"%{request.SearchTerm.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Code, term) || EF.Functions.ILike(p.Name, term));
        }

        return await query.OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectResponse(p.Id, p.Code, p.Name, p.ParentProjectId, p.ProjectTypeCode, p.ConstructionTypeCode,
                p.Location, p.DepartmentId, p.ManagerName, p.StartDate, p.EndDate, p.Description, p.Status, p.Version))
            .ToPagedResultAsync(request.PageIndex, request.PageSize, ct);
    }

    public async Task<Result<ProjectResponse>> Handle(GetProjectQuery request, CancellationToken ct) =>
        await projects.FindAll(p => p.Id == request.Id)
            .Select(p => new ProjectResponse(p.Id, p.Code, p.Name, p.ParentProjectId, p.ProjectTypeCode, p.ConstructionTypeCode,
                p.Location, p.DepartmentId, p.ManagerName, p.StartDate, p.EndDate, p.Description, p.Status, p.Version))
            .SingleOrDefaultAsync(ct) ?? throw new ProjectNotFoundException(request.Id);
}

internal sealed class ProjectCommandHandlers(IRepositoryBase<Project, Guid> projects)
    : ICommandHandler<CreateProjectCommand, Guid>,
      ICommandHandler<UpdateProjectCommand>,
      ICommandHandler<CopyProjectCommand, Guid>,
      ICommandHandler<ChangeProjectStatusCommand>,
      ICommandHandler<DeleteProjectCommand>
{
    public async Task<Result<Guid>> Handle(CreateProjectCommand r, CancellationToken ct)
    {
        await EnsureCodeFreeAsync(r.Code, ct);
        await EnsureParentExistsAsync(r.ParentProjectId, ct);
        var project = Project.Create(r.Code, new ProjectInfo(r.Name, r.ParentProjectId, r.ProjectTypeCode,
            r.ConstructionTypeCode, r.Location, r.DepartmentId, r.ManagerName, r.StartDate, r.EndDate, r.Description));
        await projects.AddAsync(project, ct);
        return project.Id;
    }

    public async Task<Result> Handle(UpdateProjectCommand r, CancellationToken ct)
    {
        var project = await projects.FindByIdAsync(r.Id, ct) ?? throw new ProjectNotFoundException(r.Id);
        project.EnsureVersion(r.Version);
        await EnsureParentExistsAsync(r.ParentProjectId, ct);
        project.Update(new ProjectInfo(r.Name, r.ParentProjectId, r.ProjectTypeCode, r.ConstructionTypeCode,
            r.Location, r.DepartmentId, r.ManagerName, r.StartDate, r.EndDate, r.Description));
        return Result.Success();
    }

    public async Task<Result<Guid>> Handle(CopyProjectCommand r, CancellationToken ct)
    {
        var source = await projects.FindAll(p => p.Id == r.Id).SingleOrDefaultAsync(ct)
                     ?? throw new ProjectNotFoundException(r.Id);
        await EnsureCodeFreeAsync(r.NewCode, ct);
        var copy = source.Copy(r.NewCode);
        await projects.AddAsync(copy, ct);
        return copy.Id;
    }

    public async Task<Result> Handle(ChangeProjectStatusCommand r, CancellationToken ct)
    {
        var project = await projects.FindByIdAsync(r.Id, ct) ?? throw new ProjectNotFoundException(r.Id);
        project.EnsureVersion(r.Version);
        project.ChangeStatus(Enum.Parse<ProjectStatus>(r.Status, ignoreCase: true));
        return Result.Success();
    }

    public async Task<Result> Handle(DeleteProjectCommand r, CancellationToken ct)
    {
        var project = await projects.FindByIdAsync(r.Id, ct) ?? throw new ProjectNotFoundException(r.Id);
        if (await projects.FindAll(p => p.ParentProjectId == r.Id).AnyAsync(ct))
            throw new ProjectHasChildrenException();

        // Kiểm tra "đã có chứng từ" (BR_EPIC_03) bổ sung khi các phân hệ chứng từ tham chiếu dự án
        projects.Remove(project);
        return Result.Success();
    }

    private async Task EnsureCodeFreeAsync(string code, CancellationToken ct)
    {
        var trimmed = code.Trim();
        if (await projects.FindAll(p => p.Code == trimmed).AnyAsync(ct))
            throw new ProjectCodeAlreadyExistsException(trimmed);
    }

    private async Task EnsureParentExistsAsync(Guid? parentId, CancellationToken ct)
    {
        if (parentId is { } id && !await projects.FindAll(p => p.Id == id).AnyAsync(ct))
            throw new ProjectNotFoundException(id);
    }
}

internal sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Vui lòng nhập mã dự án.").MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().WithMessage("Vui lòng nhập tên dự án.").MaximumLength(255);
        RuleFor(x => x.Location).MaximumLength(500);
        RuleFor(x => x.ManagerName).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

internal sealed class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Vui lòng nhập tên dự án.").MaximumLength(255);
        RuleFor(x => x.Location).MaximumLength(500);
        RuleFor(x => x.ManagerName).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

internal sealed class CopyProjectCommandValidator : AbstractValidator<CopyProjectCommand>
{
    public CopyProjectCommandValidator() =>
        RuleFor(x => x.NewCode).NotEmpty().WithMessage("Vui lòng nhập mã dự án mới.").MaximumLength(50);
}

internal sealed class ChangeProjectStatusCommandValidator : AbstractValidator<ChangeProjectStatusCommand>
{
    public ChangeProjectStatusCommandValidator() =>
        RuleFor(x => x.Status).Must(s => Enum.TryParse<ProjectStatus>(s, ignoreCase: true, out _))
            .WithMessage("Trạng thái dự án không hợp lệ.");
}
