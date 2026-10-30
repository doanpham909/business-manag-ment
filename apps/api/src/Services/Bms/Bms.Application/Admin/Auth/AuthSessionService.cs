using Bms.Application.Authorization;
using Bms.Domain.Admin.Auth;
using Bms.Domain.Admin.Companies;
using Bms.Domain.Admin.Roles;
using Bms.Domain.Admin.Users;
using BuildingBlocks.Contracts.Abstractions;
using BuildingBlocks.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bms.Application.Admin.Auth;

/// <summary>
/// Dựng phiên đăng nhập cho một người dùng: công ty được vào, quyền hiệu lực tại công ty được chọn,
/// access token và refresh token mới. Dùng chung cho đăng nhập, chọn công ty, làm mới phiên.
/// Các truy vấn tự lọc theo TenantId của người dùng vì lúc đăng nhập chưa có tenant trong ngữ cảnh.
/// </summary>
internal sealed class AuthSessionService(
    IRepositoryBase<Company, Guid> companies,
    IRepositoryBase<Role, Guid> roles,
    IRepositoryBase<RefreshToken, Guid> refreshTokens,
    IAccessTokenIssuer tokenIssuer,
    TimeProvider timeProvider)
{
    public async Task<IReadOnlyList<CompanyOption>> GetCompaniesAsync(User user, CancellationToken ct)
    {
        var ids = user.CompanyAccesses.Select(a => a.CompanyId).ToList();
        return await companies.FindAll()
            .IgnoreQueryFilters([QueryFilterNames.Tenant]) // ngữ cảnh đăng nhập: lọc tay theo tenant của user
            .Where(c => c.TenantId == user.TenantId && ids.Contains(c.Id) && c.Status == CompanyStatus.Active)
            .OrderBy(c => c.Name)
            .Select(c => new CompanyOption(c.Id, c.Code, c.Name))
            .ToListAsync(ct);
    }

    /// <summary>Quyền hiệu lực: hợp các vai trò của user tại các công ty cho trước.</summary>
    public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(
        User user, IReadOnlyCollection<Guid> companyIds, CancellationToken ct)
    {
        var roleIds = user.RoleAssignments.Where(r => companyIds.Contains(r.CompanyId)).Select(r => r.RoleId).ToList();
        if (roleIds.Count == 0)
            return [];

        var assigned = await roles.FindAll(null, r => r.Permissions)
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .Where(r => r.TenantId == user.TenantId && roleIds.Contains(r.Id))
            .ToListAsync(ct);

        return assigned.Any(r => r.IsSystem)
            ? RoleTemplates.All.First(t => t.IsSystem).PermissionCodes.ToHashSet()
            : assigned.SelectMany(r => r.Permissions.Select(p => p.PermissionCode)).ToHashSet();
    }

    /// <summary>Có quyền xem Tất cả công ty khi một vai trò ở bất kỳ công ty nào có admin.company.view-all.</summary>
    public async Task<bool> CanViewAllAsync(User user, IReadOnlyList<CompanyOption> available, CancellationToken ct)
    {
        var permissions = await GetPermissionsAsync(user, available.Select(c => c.Id).ToList(), ct);
        return permissions.Contains(AdminPermissions.CompanyViewAll);
    }

    public async Task<AuthSession> IssueAsync(
        User user, Guid? companyId, bool allCompanies, IReadOnlyList<CompanyOption> available, CancellationToken ct)
    {
        var readable = allCompanies ? available.Select(c => c.Id).ToList()
            : companyId is { } id ? [id] : [];
        var permissions = await GetPermissionsAsync(user, readable, ct);
        var access = tokenIssuer.Issue(new AccessTokenRequest(
            user.Id, user.TenantId, user.Username, user.FullName,
            allCompanies ? null : companyId, allCompanies ? readable : [], permissions));

        var (refresh, hash) = tokenIssuer.CreateRefreshToken();
        await refreshTokens.AddAsync(RefreshToken.Create(user.Id, user.TenantId, allCompanies ? null : companyId,
            allCompanies, hash, timeProvider.GetUtcNow(), AuthDefaults.RefreshTokenLifetime), ct);

        var current = allCompanies ? null : available.FirstOrDefault(c => c.Id == companyId);
        return new AuthSession(
            access.Token, access.ExpiresAt, refresh,
            new SessionInfo(new SessionUser(user.Id, user.Username, user.FullName), available, current, allCompanies,
                await CanViewAllAsync(user, available, ct), permissions.Order().ToList()));
    }
}
