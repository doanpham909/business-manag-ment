using Bms.Application.Authorization;
using Bms.Application.Common.Paging;
using Bms.Domain.Admin.Roles;
using Bms.Domain.Admin.Users;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Contracts.Persistence;
using BuildingBlocks.Shared.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Bms.Application.Admin.Roles;

public sealed record RoleListItem(Guid Id, string Code, string Name, string? Description, bool IsSystem, int UserCount);

public sealed record RoleDataScopeInput(string Module, DataScope Scope);

public sealed record RoleDetail(
    Guid Id, string Code, string Name, string? Description, bool IsSystem,
    IReadOnlyList<string> Permissions, IReadOnlyList<RoleDataScopeInput> DataScopes, uint Version);

public sealed record PermissionCatalogResponse(
    IReadOnlyDictionary<string, string> Modules, IReadOnlyList<PermissionResource> Resources);

public sealed record GetRolesQuery(int PageIndex = 1, int PageSize = 50, string? SearchTerm = null)
    : IQuery<PagedResult<RoleListItem>>;

public sealed record GetRoleQuery(Guid Id) : IQuery<RoleDetail>;

public sealed record GetPermissionCatalogQuery : IQuery<PermissionCatalogResponse>;

public sealed record CreateRoleCommand(
    string Code, string Name, string? Description,
    IReadOnlyList<string> Permissions, IReadOnlyList<RoleDataScopeInput> DataScopes) : ICommand<Guid>;

public sealed record UpdateRoleCommand(
    Guid Id, string Name, string? Description,
    IReadOnlyList<string> Permissions, IReadOnlyList<RoleDataScopeInput> DataScopes, uint? Version) : ICommand;

public sealed record DeleteRoleCommand(Guid Id) : ICommand;

internal sealed class RoleQueryHandlers(IRepositoryBase<Role, Guid> roles, IRepositoryBase<User, Guid> users)
    : IQueryHandler<GetRolesQuery, PagedResult<RoleListItem>>,
      IQueryHandler<GetRoleQuery, RoleDetail>,
      IQueryHandler<GetPermissionCatalogQuery, PermissionCatalogResponse>
{
    public async Task<Result<PagedResult<RoleListItem>>> Handle(GetRolesQuery request, CancellationToken ct)
    {
        var query = roles.FindAll();
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = $"%{request.SearchTerm.Trim()}%";
            query = query.Where(r => EF.Functions.ILike(r.Code, term) || EF.Functions.ILike(r.Name, term));
        }

        var userRoles = users.FindAll().SelectMany(u => u.RoleAssignments);
        return await query.OrderBy(r => r.Name)
            .Select(r => new RoleListItem(r.Id, r.Code, r.Name, r.Description, r.IsSystem,
                userRoles.Where(a => a.RoleId == r.Id).Select(a => a.UserId).Distinct().Count()))
            .ToPagedResultAsync(request.PageIndex, request.PageSize, ct);
    }

    public async Task<Result<RoleDetail>> Handle(GetRoleQuery request, CancellationToken ct)
    {
        var role = await roles.FindAll(r => r.Id == request.Id, r => r.Permissions, r => r.DataScopes)
                       .SingleOrDefaultAsync(ct) ?? throw new RoleNotFoundException(request.Id);

        var permissions = role.IsSystem
            ? RoleTemplates.All.First(t => t.IsSystem).PermissionCodes.ToList()
            : role.Permissions.Select(p => p.PermissionCode).ToList();
        return new RoleDetail(role.Id, role.Code, role.Name, role.Description, role.IsSystem, permissions,
            role.DataScopes.Select(s => new RoleDataScopeInput(s.Module, s.Scope)).ToList(), role.Version);
    }

    public Task<Result<PermissionCatalogResponse>> Handle(GetPermissionCatalogQuery request, CancellationToken ct) =>
        Task.FromResult<Result<PermissionCatalogResponse>>(
            new PermissionCatalogResponse(PermissionCatalog.ModuleNames, PermissionCatalog.Resources));
}

internal sealed class CreateRoleCommandHandler(IRepositoryBase<Role, Guid> roles) : ICommandHandler<CreateRoleCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken ct)
    {
        var code = request.Code.Trim();
        if (await roles.FindAll(r => r.Code == code).AnyAsync(ct))
            throw new RoleCodeAlreadyExistsException(code);

        var role = Role.Create(code, request.Name, request.Description);
        role.SetPermissions(request.Permissions.Where(PermissionCatalog.AllCodes.Contains));
        role.SetDataScopes(request.DataScopes.Select(s => (s.Module, s.Scope)));
        await roles.AddAsync(role, ct);
        return role.Id;
    }
}

internal sealed class UpdateRoleCommandHandler(IRepositoryBase<Role, Guid> roles) : ICommandHandler<UpdateRoleCommand>
{
    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken ct)
    {
        var role = await roles.FindByIdAsync(request.Id, ct, r => r.Permissions, r => r.DataScopes)
                   ?? throw new RoleNotFoundException(request.Id);
        role.EnsureVersion(request.Version);
        if (role.IsSystem)
            throw new RoleIsSystemException();

        role.Update(request.Name, request.Description);
        role.SetPermissions(request.Permissions.Where(PermissionCatalog.AllCodes.Contains));
        role.SetDataScopes(request.DataScopes.Select(s => (s.Module, s.Scope)));
        return Result.Success();
    }
}

internal sealed class DeleteRoleCommandHandler(IRepositoryBase<Role, Guid> roles, IRepositoryBase<User, Guid> users)
    : ICommandHandler<DeleteRoleCommand>
{
    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken ct)
    {
        var role = await roles.FindByIdAsync(request.Id, ct) ?? throw new RoleNotFoundException(request.Id);
        if (role.IsSystem)
            throw new RoleIsSystemException();
        if (await users.FindAll().AnyAsync(u => u.RoleAssignments.Any(a => a.RoleId == request.Id), ct))
            throw new RoleInUseException();

        roles.Remove(role);
        return Result.Success();
    }
}

internal sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Vui lòng nhập mã vai trò.").MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().WithMessage("Vui lòng nhập tên vai trò.").MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

internal sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Vui lòng nhập tên vai trò.").MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
