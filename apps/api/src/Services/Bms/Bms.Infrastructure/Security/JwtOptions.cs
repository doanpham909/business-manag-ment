namespace Bms.Infrastructure.Security;

/// <summary>Cấu hình JWT (section "Jwt"). SigningKey đặt qua user-secrets / biến môi trường, không commit.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "bms-api";
    public string Audience { get; set; } = "bms-web";

    /// <summary>Khóa ký HMAC-SHA256, tối thiểu 32 byte.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
