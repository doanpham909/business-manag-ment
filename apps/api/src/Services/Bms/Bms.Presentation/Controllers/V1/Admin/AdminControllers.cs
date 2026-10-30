using Bms.Application.Admin;
using Bms.Application.Admin.AuditLogs;
using Bms.Application.Admin.Catalogs;
using Bms.Application.Admin.Companies;
using Bms.Application.Admin.Roles;
using Bms.Application.Admin.Users;
using Bms.Presentation.Abstractions;
using BuildingBlocks.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bms.Presentation.Controllers.V1.Admin;

/// <summary>Công ty trong khách hàng (ADM-00).</summary>
[Route($"{AppConstants.ApiV1Prefix}/admin/companies")]
public sealed class CompanyController(ISender sender) : ApiControllerBase(sender)
{
    public sealed record ActiveRequest(bool IsActive);

    [HttpGet, HasPermission(AdminPermissions.CompanyView)]
    public Task<IActionResult> List([FromQuery] GetCompaniesQuery query, CancellationToken ct) => SendAsync(query, ct);

    [HttpPost, HasPermission(AdminPermissions.CompanyCreate)]
    public Task<IActionResult> Create([FromBody] CreateCompanyCommand command, CancellationToken ct) => SendAsync(command, ct);

    [HttpPut("{id:guid}"), HasPermission(AdminPermissions.CompanyUpdate)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyCommand command, CancellationToken ct) =>
        SendAsync(command with { Id = id }, ct);

    [HttpPost("{id:guid}/active"), HasPermission(AdminPermissions.CompanyUpdate)]
    public Task<IActionResult> SetActive(Guid id, [FromBody] ActiveRequest request, CancellationToken ct) =>
        SendAsync(new SetCompanyActiveCommand(id, request.IsActive), ct);
}

/// <summary>Người dùng (ADM-01).</summary>
[Route($"{AppConstants.ApiV1Prefix}/admin/users")]
public sealed class UserController(ISender sender) : ApiControllerBase(sender)
{
    public sealed record ResetPasswordRequest(string NewPassword);

    [HttpGet, HasPermission(AdminPermissions.UserView)]
    public Task<IActionResult> List([FromQuery] GetUsersQuery query, CancellationToken ct) => SendAsync(query, ct);

    [HttpGet("{id:guid}"), HasPermission(AdminPermissions.UserView)]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) => SendAsync(new GetUserQuery(id), ct);

    [HttpPost, HasPermission(AdminPermissions.UserCreate)]
    public Task<IActionResult> Create([FromBody] CreateUserCommand command, CancellationToken ct) => SendAsync(command, ct);

    [HttpPut("{id:guid}"), HasPermission(AdminPermissions.UserUpdate)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateUserCommand command, CancellationToken ct) =>
        SendAsync(command with { Id = id }, ct);

    [HttpPost("{id:guid}/lock"), HasPermission(AdminPermissions.UserUpdate)]
    public Task<IActionResult> Lock(Guid id, CancellationToken ct) => SendAsync(new SetUserLockedCommand(id, true), ct);

    [HttpPost("{id:guid}/unlock"), HasPermission(AdminPermissions.UserUpdate)]
    public Task<IActionResult> Unlock(Guid id, CancellationToken ct) => SendAsync(new SetUserLockedCommand(id, false), ct);

    [HttpPost("{id:guid}/reset-password"), HasPermission(AdminPermissions.UserUpdate)]
    public Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request, CancellationToken ct) =>
        SendAsync(new ResetUserPasswordCommand(id, request.NewPassword), ct);
}

/// <summary>Vai trò, quyền và phạm vi dữ liệu (ADM-02, ADM-03).</summary>
[Route($"{AppConstants.ApiV1Prefix}/admin/roles")]
public sealed class RoleController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet, HasPermission(AdminPermissions.RoleView)]
    public Task<IActionResult> List([FromQuery] GetRolesQuery query, CancellationToken ct) => SendAsync(query, ct);

    [HttpGet("permission-catalog"), HasPermission(AdminPermissions.RoleView)]
    public Task<IActionResult> Catalog(CancellationToken ct) => SendAsync(new GetPermissionCatalogQuery(), ct);

    [HttpGet("{id:guid}"), HasPermission(AdminPermissions.RoleView)]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) => SendAsync(new GetRoleQuery(id), ct);

    [HttpPost, HasPermission(AdminPermissions.RoleCreate)]
    public Task<IActionResult> Create([FromBody] CreateRoleCommand command, CancellationToken ct) => SendAsync(command, ct);

    [HttpPut("{id:guid}"), HasPermission(AdminPermissions.RoleUpdate)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleCommand command, CancellationToken ct) =>
        SendAsync(command with { Id = id }, ct);

    [HttpDelete("{id:guid}"), HasPermission(AdminPermissions.RoleDelete)]
    public Task<IActionResult> Delete(Guid id, CancellationToken ct) => SendAsync(new DeleteRoleCommand(id), ct);
}

/// <summary>Danh mục dùng chung (ADM-05).</summary>
[Route($"{AppConstants.ApiV1Prefix}/admin/catalogs")]
public sealed class CatalogController(ISender sender) : ApiControllerBase(sender)
{
    public sealed record ActiveRequest(bool IsActive);

    [HttpGet("types"), HasPermission(AdminPermissions.CatalogView)]
    public Task<IActionResult> Types(CancellationToken ct) => SendAsync(new GetCatalogTypesQuery(), ct);

    [HttpGet, HasPermission(AdminPermissions.CatalogView)]
    public Task<IActionResult> List([FromQuery] GetCatalogItemsQuery query, CancellationToken ct) => SendAsync(query, ct);

    [HttpPost, HasPermission(AdminPermissions.CatalogCreate)]
    public Task<IActionResult> Create([FromBody] CreateCatalogItemCommand command, CancellationToken ct) => SendAsync(command, ct);

    [HttpPut("{id:guid}"), HasPermission(AdminPermissions.CatalogUpdate)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateCatalogItemCommand command, CancellationToken ct) =>
        SendAsync(command with { Id = id }, ct);

    [HttpPost("{id:guid}/active"), HasPermission(AdminPermissions.CatalogUpdate)]
    public Task<IActionResult> SetActive(Guid id, [FromBody] ActiveRequest request, CancellationToken ct) =>
        SendAsync(new SetCatalogItemActiveCommand(id, request.IsActive), ct);
}

/// <summary>Nhật ký thay đổi (ADM-08).</summary>
[Route($"{AppConstants.ApiV1Prefix}/admin/audit-logs")]
public sealed class AuditLogController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet, HasPermission(AdminPermissions.AuditLogView)]
    public Task<IActionResult> List([FromQuery] GetAuditLogsQuery query, CancellationToken ct) => SendAsync(query, ct);
}
