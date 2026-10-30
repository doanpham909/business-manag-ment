using BuildingBlocks.Contracts.Abstractions;
using BuildingBlocks.Shared;
using Microsoft.AspNetCore.Http;

namespace Bms.Infrastructure.Identity;

/// <summary>
/// Công ty của request lấy từ claim của user ĐÃ XÁC THỰC (ADR-0002):
/// "company_id" khi đã chọn công ty; "all_companies" + các "readable_company" ở chế độ Tất cả công ty.
/// Không bao giờ đọc từ header/query string.
/// </summary>
internal sealed class CompanyContext(IHttpContextAccessor httpContextAccessor) : ICompanyContext
{
    private Guid? ActivePrincipalCompany
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;
            return principal?.Identity?.IsAuthenticated == true
                ? ClaimParser.ParseGuid(principal, ClaimNames.CompanyId)
                : null;
        }
    }

    public Guid? ActiveCompanyId => IsAllCompanies ? null : ActivePrincipalCompany;

    public bool IsAllCompanies
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;
            return principal?.Identity?.IsAuthenticated == true
                   && principal.HasClaim(ClaimNames.AllCompanies, "true");
        }
    }

    public IReadOnlyList<Guid> ReadableCompanyIds
    {
        get
        {
            if (ActiveCompanyId is { } active)
                return [active];

            if (!IsAllCompanies)
                return [];

            return httpContextAccessor.HttpContext!.User.FindAll(ClaimNames.ReadableCompany)
                .Select(c => Guid.TryParse(c.Value, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToList();
        }
    }
}
