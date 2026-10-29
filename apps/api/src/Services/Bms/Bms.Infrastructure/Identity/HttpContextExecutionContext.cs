using System.Security.Claims;
using BuildingBlocks.Contracts.Abstractions;
using BuildingBlocks.Shared;
using Microsoft.AspNetCore.Http;

namespace Bms.Infrastructure.Identity;

/// <summary>
/// Người dùng hiện tại lấy từ claim "sub" của HttpContext.User
/// (fallback ClaimTypes.NameIdentifier vì JwtBearer mặc định map "sub" sang claim này).
/// </summary>
internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId =>
        IsAuthenticated
            ? ClaimParser.ParseGuid(Principal!, ClaimNames.Subject, ClaimTypes.NameIdentifier)
            : null;
}

/// <summary>
/// Tenant hiện tại lấy từ claim "tenant_id" của user ĐÃ XÁC THỰC.
/// Bảo mật: KHÔNG đọc tenant từ header/query string do client tự gửi — tránh giả mạo tenant.
/// </summary>
internal sealed class TenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    public Guid? TenantId
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true)
                return null;

            return ClaimParser.ParseGuid(principal, ClaimNames.TenantId);
        }
    }
}

/// <summary>Đọc claim dạng Guid; claim thiếu hoặc sai định dạng → null (không ném lỗi).</summary>
internal static class ClaimParser
{
    public static Guid? ParseGuid(ClaimsPrincipal principal, params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = principal.FindFirstValue(claimType);
            if (Guid.TryParse(value, out var id) && id != Guid.Empty)
                return id;
        }

        return null;
    }
}
