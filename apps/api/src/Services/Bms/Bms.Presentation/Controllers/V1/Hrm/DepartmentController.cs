using Bms.Application.Hrm.Departments;
using Bms.Presentation.Abstractions;
using BuildingBlocks.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bms.Presentation.Controllers.V1.Hrm;

/// <summary>Cơ cấu tổ chức của công ty đang làm việc (HRM-01).</summary>
[Route($"{AppConstants.ApiV1Prefix}/hrm/departments")]
public sealed class DepartmentController(ISender sender) : ApiControllerBase(sender)
{
    public sealed record ActiveRequest(bool IsActive);

    [HttpGet, HasPermission(HrmPermissions.DepartmentView)]
    public Task<IActionResult> List([FromQuery] GetDepartmentsQuery query, CancellationToken ct) => SendAsync(query, ct);

    [HttpPost, HasPermission(HrmPermissions.DepartmentCreate)]
    public Task<IActionResult> Create([FromBody] CreateDepartmentCommand command, CancellationToken ct) => SendAsync(command, ct);

    [HttpPut("{id:guid}"), HasPermission(HrmPermissions.DepartmentUpdate)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentCommand command, CancellationToken ct) =>
        SendAsync(command with { Id = id }, ct);

    [HttpPost("{id:guid}/active"), HasPermission(HrmPermissions.DepartmentUpdate)]
    public Task<IActionResult> SetActive(Guid id, [FromBody] ActiveRequest request, CancellationToken ct) =>
        SendAsync(new SetDepartmentActiveCommand(id, request.IsActive), ct);

    [HttpDelete("{id:guid}"), HasPermission(HrmPermissions.DepartmentDelete)]
    public Task<IActionResult> Delete(Guid id, CancellationToken ct) => SendAsync(new DeleteDepartmentCommand(id), ct);
}
