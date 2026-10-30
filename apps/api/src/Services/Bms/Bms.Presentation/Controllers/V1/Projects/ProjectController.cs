using Bms.Application.Projects;
using Bms.Presentation.Abstractions;
using BuildingBlocks.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bms.Presentation.Controllers.V1.Projects;

/// <summary>Danh mục dự án của công ty đang làm việc (PRJ-01): thêm, sửa, xóa, sao chép, đổi trạng thái.</summary>
[Route($"{AppConstants.ApiV1Prefix}/projects/projects")]
public sealed class ProjectController(ISender sender) : ApiControllerBase(sender)
{
    public sealed record CopyRequest(string NewCode);

    public sealed record StatusRequest(string Status, uint? Version);

    [HttpGet, HasPermission(ProjectPermissions.View)]
    public Task<IActionResult> List([FromQuery] GetProjectsQuery query, CancellationToken ct) => SendAsync(query, ct);

    [HttpGet("{id:guid}"), HasPermission(ProjectPermissions.View)]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) => SendAsync(new GetProjectQuery(id), ct);

    [HttpPost, HasPermission(ProjectPermissions.Create)]
    public Task<IActionResult> Create([FromBody] CreateProjectCommand command, CancellationToken ct) => SendAsync(command, ct);

    [HttpPut("{id:guid}"), HasPermission(ProjectPermissions.Update)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectCommand command, CancellationToken ct) =>
        SendAsync(command with { Id = id }, ct);

    [HttpPost("{id:guid}/copy"), HasPermission(ProjectPermissions.Create)]
    public Task<IActionResult> Copy(Guid id, [FromBody] CopyRequest request, CancellationToken ct) =>
        SendAsync(new CopyProjectCommand(id, request.NewCode), ct);

    [HttpPost("{id:guid}/status"), HasPermission(ProjectPermissions.Update)]
    public Task<IActionResult> ChangeStatus(Guid id, [FromBody] StatusRequest request, CancellationToken ct) =>
        SendAsync(new ChangeProjectStatusCommand(id, request.Status, request.Version), ct);

    [HttpDelete("{id:guid}"), HasPermission(ProjectPermissions.Delete)]
    public Task<IActionResult> Delete(Guid id, CancellationToken ct) => SendAsync(new DeleteProjectCommand(id), ct);
}
