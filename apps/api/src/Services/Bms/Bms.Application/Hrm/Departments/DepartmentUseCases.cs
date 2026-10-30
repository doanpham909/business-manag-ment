using Bms.Domain.Hrm.Departments;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Contracts.Persistence;
using BuildingBlocks.Shared.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Bms.Application.Hrm.Departments;

public static class HrmPermissions
{
    public const string DepartmentView = "hrm.department.view";
    public const string DepartmentCreate = "hrm.department.create";
    public const string DepartmentUpdate = "hrm.department.update";
    public const string DepartmentDelete = "hrm.department.delete";
}

public sealed record DepartmentResponse(
    Guid Id, string Code, string Name, Guid? ParentId, string? TypeCode, int SortOrder, bool IsActive, uint Version);

/// <summary>Toàn bộ đơn vị của công ty (FE dựng cây từ ParentId).</summary>
public sealed record GetDepartmentsQuery(bool ActiveOnly = false) : IQuery<IReadOnlyList<DepartmentResponse>>;

public sealed record CreateDepartmentCommand(string Code, string Name, Guid? ParentId, string? TypeCode, int SortOrder)
    : ICommand<Guid>;

public sealed record UpdateDepartmentCommand(
    Guid Id, string Name, Guid? ParentId, string? TypeCode, int SortOrder, uint? Version) : ICommand;

public sealed record SetDepartmentActiveCommand(Guid Id, bool IsActive) : ICommand;

public sealed record DeleteDepartmentCommand(Guid Id) : ICommand;

internal sealed class GetDepartmentsQueryHandler(IRepositoryBase<Department, Guid> departments)
    : IQueryHandler<GetDepartmentsQuery, IReadOnlyList<DepartmentResponse>>
{
    public async Task<Result<IReadOnlyList<DepartmentResponse>>> Handle(GetDepartmentsQuery request, CancellationToken ct)
    {
        var query = departments.FindAll();
        if (request.ActiveOnly)
            query = query.Where(d => d.IsActive);

        IReadOnlyList<DepartmentResponse> items = await query.OrderBy(d => d.SortOrder).ThenBy(d => d.Name)
            .Select(d => new DepartmentResponse(d.Id, d.Code, d.Name, d.ParentId, d.TypeCode, d.SortOrder, d.IsActive, d.Version))
            .ToListAsync(ct);
        return Result.Success(items);
    }
}

internal sealed class CreateDepartmentCommandHandler(IRepositoryBase<Department, Guid> departments)
    : ICommandHandler<CreateDepartmentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateDepartmentCommand request, CancellationToken ct)
    {
        var code = request.Code.Trim();
        if (await departments.FindAll(d => d.Code == code).AnyAsync(ct))
            throw new DepartmentCodeAlreadyExistsException(code);

        Department? parent = null;
        if (request.ParentId is { } parentId)
            parent = await departments.FindAll(d => d.Id == parentId).SingleOrDefaultAsync(ct)
                     ?? throw new DepartmentNotFoundException(parentId);

        var department = Department.Create(code, request.Name, parent, request.TypeCode, request.SortOrder);
        await departments.AddAsync(department, ct);
        return department.Id;
    }
}

internal sealed class UpdateDepartmentCommandHandler(IRepositoryBase<Department, Guid> departments)
    : ICommandHandler<UpdateDepartmentCommand>
{
    public async Task<Result> Handle(UpdateDepartmentCommand request, CancellationToken ct)
    {
        var department = await departments.FindByIdAsync(request.Id, ct) ?? throw new DepartmentNotFoundException(request.Id);
        department.EnsureVersion(request.Version);
        department.Update(request.Name, request.TypeCode, request.SortOrder);

        if (department.ParentId != request.ParentId)
            await MoveWithDescendantsAsync(department, request.ParentId, ct);

        return Result.Success();
    }

    /// <summary>Đổi cha rồi cập nhật Path của toàn bộ đơn vị cấp dưới (lấy một lần, không query trong vòng lặp).</summary>
    private async Task MoveWithDescendantsAsync(Department department, Guid? parentId, CancellationToken ct)
    {
        Department? parent = null;
        if (parentId is { } id)
            parent = await departments.FindAll(d => d.Id == id).SingleOrDefaultAsync(ct)
                     ?? throw new DepartmentNotFoundException(id);

        var oldPath = department.Path;
        var descendants = await departments.FindAllTracking(d => d.Path.StartsWith(oldPath) && d.Id != department.Id)
            .OrderBy(d => d.Path.Length).ToListAsync(ct);

        department.MoveTo(parent);
        var byId = descendants.ToDictionary(d => d.Id);
        byId[department.Id] = department;
        foreach (var child in descendants)
            child.MoveTo(byId[child.ParentId!.Value]);
    }
}

internal sealed class SetDepartmentActiveCommandHandler(IRepositoryBase<Department, Guid> departments)
    : ICommandHandler<SetDepartmentActiveCommand>
{
    public async Task<Result> Handle(SetDepartmentActiveCommand request, CancellationToken ct)
    {
        var department = await departments.FindByIdAsync(request.Id, ct) ?? throw new DepartmentNotFoundException(request.Id);
        department.SetActive(request.IsActive);
        return Result.Success();
    }
}

internal sealed class DeleteDepartmentCommandHandler(IRepositoryBase<Department, Guid> departments)
    : ICommandHandler<DeleteDepartmentCommand>
{
    public async Task<Result> Handle(DeleteDepartmentCommand request, CancellationToken ct)
    {
        var department = await departments.FindByIdAsync(request.Id, ct) ?? throw new DepartmentNotFoundException(request.Id);
        if (await departments.FindAll(d => d.ParentId == request.Id).AnyAsync(ct))
            throw new DepartmentHasChildrenException();

        departments.Remove(department);
        return Result.Success();
    }
}

internal sealed class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Vui lòng nhập mã đơn vị.").MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().WithMessage("Vui lòng nhập tên đơn vị.").MaximumLength(255);
        RuleFor(x => x.TypeCode).MaximumLength(50);
    }
}

internal sealed class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Vui lòng nhập tên đơn vị.").MaximumLength(255);
        RuleFor(x => x.TypeCode).MaximumLength(50);
    }
}
