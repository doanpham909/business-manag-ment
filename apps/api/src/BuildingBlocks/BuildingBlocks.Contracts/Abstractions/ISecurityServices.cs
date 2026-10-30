namespace BuildingBlocks.Contracts.Abstractions;

/// <summary>Băm và kiểm tra mật khẩu (thuật toán chậm, không tự viết).</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string hash, string password);
}

/// <summary>Thông tin đưa vào access token.</summary>
public sealed record AccessTokenRequest(
    Guid UserId,
    Guid TenantId,
    string Username,
    string FullName,
    Guid? CompanyId,
    IReadOnlyList<Guid> AllCompanyIds,
    IReadOnlyCollection<string> Permissions);

/// <summary>Access token đã cấp.</summary>
public sealed record IssuedAccessToken(string Token, DateTimeOffset ExpiresAt);

/// <summary>Cấp access token (JWT) và sinh refresh token ngẫu nhiên.</summary>
public interface IAccessTokenIssuer
{
    IssuedAccessToken Issue(AccessTokenRequest request);

    /// <summary>Refresh token dạng chuỗi ngẫu nhiên (trả cho client) và mã băm (lưu DB).</summary>
    (string Token, string Hash) CreateRefreshToken();

    string HashRefreshToken(string token);
}
