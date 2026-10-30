using Bms.Application.Admin.Auth;
using Bms.Application.Admin.Auth.Commands;
using Bms.Application.Admin.Auth.Queries;
using Bms.Presentation.Abstractions;
using BuildingBlocks.Shared;
using BuildingBlocks.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bms.Presentation.Controllers.V1.Admin;

/// <summary>Đăng nhập, chọn công ty, làm mới phiên, đăng xuất (CMN-01). Refresh token nằm trong cookie HttpOnly.</summary>
[Route($"{AppConstants.ApiV1Prefix}/auth")]
public sealed class AuthController(ISender sender) : ApiControllerBase(sender)
{
    private const string RefreshCookie = "bms_rt";
    private const string CookiePath = "/api/v1/auth";

    public sealed record SelectCompanyRequest(Guid? CompanyId, bool AllCompanies);

    /// <summary>Đăng nhập. Chỉ được vào một công ty thì chọn sẵn công ty đó.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct) => IssueAsync(command, ct);

    /// <summary>Chọn / đổi công ty, hoặc vào chế độ Tất cả công ty (chỉ đọc).</summary>
    [Authorize]
    [HttpPost("select-company")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> SelectCompany([FromBody] SelectCompanyRequest request, CancellationToken ct) =>
        IssueAsync(new SelectCompanyCommand(request.CompanyId, request.AllCompanies, Request.Cookies[RefreshCookie]), ct);

    /// <summary>Làm mới access token bằng refresh token trong cookie.</summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Refresh(CancellationToken ct) =>
        IssueAsync(new RefreshSessionCommand(Request.Cookies[RefreshCookie] ?? string.Empty), ct);

    /// <summary>Đăng xuất: thu hồi refresh token và xóa cookie.</summary>
    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await Sender.Send(new LogoutCommand(Request.Cookies[RefreshCookie]), ct);
        Response.Cookies.Delete(RefreshCookie, CookieOptions(DateTimeOffset.UnixEpoch));
        return Ok(Result.Success());
    }

    /// <summary>Thông tin phiên hiện tại: người dùng, công ty được vào, công ty đang làm việc, quyền.</summary>
    [Authorize]
    [HttpGet("session")]
    [ProducesResponseType(typeof(Result<SessionInfo>), StatusCodes.Status200OK)]
    public Task<IActionResult> Session(CancellationToken ct) => SendAsync(new GetSessionQuery(), ct);

    private async Task<IActionResult> IssueAsync(MediatR.IRequest<Result<AuthSession>> command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        if (result.IsFailure)
            return HandleFailure(result);

        var session = result.Value;
        Response.Cookies.Append(RefreshCookie, session.RefreshToken,
            CookieOptions(DateTimeOffset.UtcNow.Add(AuthDefaults.RefreshTokenLifetime)));
        return Ok(Result.Success(new AuthResponse(session.AccessToken, session.ExpiresAt, session.Session)));
    }

    private static CookieOptions CookieOptions(DateTimeOffset expires) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = CookiePath,
        Expires = expires,
    };
}
