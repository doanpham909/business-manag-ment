using Bms.Application.SystemInfo.Queries.GetSystemInfo;
using Bms.Presentation.Abstractions;
using BuildingBlocks.Shared;
using BuildingBlocks.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bms.Presentation.Controllers.V1;

/// <summary>Thông tin hệ thống (endpoint mẫu cho luồng Controller → MediatR → Handler).</summary>
[Route($"{AppConstants.ApiV1Prefix}/system")]
public sealed class SystemController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Lấy tên ứng dụng, version, môi trường và giờ server (UTC).</summary>
    [HttpGet("info")]
    [ProducesResponseType(typeof(Result<SystemInfoResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetInfo(CancellationToken cancellationToken)
        => SendAsync(new GetSystemInfoQuery(), cancellationToken);
}
