namespace Bms.Application.Admin.Auth;

public static class AuthDefaults
{
    /// <summary>Thời hạn refresh token (cookie HttpOnly).</summary>
    public static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);
}

public sealed record CompanyOption(Guid Id, string Code, string Name);

public sealed record SessionUser(Guid Id, string Username, string FullName);

/// <summary>Thông tin phiên trả cho FE: người dùng, công ty được vào, công ty đang làm việc, quyền.</summary>
public sealed record SessionInfo(
    SessionUser User,
    IReadOnlyList<CompanyOption> Companies,
    CompanyOption? CurrentCompany,
    bool AllCompanies,
    bool CanViewAllCompanies,
    IReadOnlyList<string> Permissions);

/// <summary>
/// Kết quả đăng nhập / chọn công ty / làm mới phiên. RefreshToken chỉ để controller ghi cookie HttpOnly,
/// không trả trong body.
/// </summary>
public sealed record AuthSession(string AccessToken, DateTimeOffset ExpiresAt, string RefreshToken, SessionInfo Session);

/// <summary>Body trả về cho FE (không có refresh token).</summary>
public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, SessionInfo Session);
