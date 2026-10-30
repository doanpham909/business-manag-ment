using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Contracts.Abstractions;
using BuildingBlocks.Shared;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Bms.Infrastructure.Security;

/// <summary>Cấp access token JWT (HMAC-SHA256) và sinh refresh token ngẫu nhiên 64 byte.</summary>
internal sealed class JwtAccessTokenIssuer(IOptions<JwtOptions> options, TimeProvider timeProvider)
    : IAccessTokenIssuer
{
    private readonly JwtOptions _options = options.Value;

    public IssuedAccessToken Issue(AccessTokenRequest request)
    {
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(ClaimNames.Subject, request.UserId.ToString()),
            new(ClaimNames.TenantId, request.TenantId.ToString()),
            new(ClaimNames.Username, request.Username),
            new(ClaimNames.Name, request.FullName),
        };

        if (request.CompanyId is { } companyId)
        {
            claims.Add(new Claim(ClaimNames.CompanyId, companyId.ToString()));
        }
        else if (request.AllCompanyIds.Count > 0)
        {
            claims.Add(new Claim(ClaimNames.AllCompanies, "true"));
            claims.AddRange(request.AllCompanyIds.Select(id => new Claim(ClaimNames.ReadableCompany, id.ToString())));
        }

        claims.AddRange(request.Permissions.Select(p => new Claim(ClaimNames.Permission, p)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(SigningKey(_options), SecurityAlgorithms.HmacSha256),
        };

        return new IssuedAccessToken(new JsonWebTokenHandler().CreateToken(descriptor), expiresAt);
    }

    public (string Token, string Hash) CreateRefreshToken()
    {
        var token = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        return (token, HashRefreshToken(token));
    }

    public string HashRefreshToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    internal static SymmetricSecurityKey SigningKey(JwtOptions options) =>
        new(Encoding.UTF8.GetBytes(options.SigningKey));
}

/// <summary>Băm mật khẩu bằng PasswordHasher của ASP.NET Core Identity (PBKDF2, nhiều vòng lặp).</summary>
internal sealed class IdentityPasswordHasher : IPasswordHasher
{
    private static readonly Microsoft.AspNetCore.Identity.PasswordHasher<object> Hasher = new();
    private static readonly object Owner = new();

    public string Hash(string password) => Hasher.HashPassword(Owner, password);

    public bool Verify(string hash, string password) =>
        Hasher.VerifyHashedPassword(Owner, hash, password)
            is not Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed;
}
