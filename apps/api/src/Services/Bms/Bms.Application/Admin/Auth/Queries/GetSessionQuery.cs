using Bms.Application.Admin.Auth.Commands;
using Bms.Domain.Admin.Users;
using BuildingBlocks.Contracts.Abstractions;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Contracts.Persistence;
using BuildingBlocks.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Bms.Application.Admin.Auth.Queries;

/// <summary>Thông tin phiên hiện tại (người dùng, công ty, quyền) — tính lại từ dữ liệu, không tin token cũ.</summary>
public sealed record GetSessionQuery : IQuery<SessionInfo>;

internal sealed class GetSessionQueryHandler(
    IRepositoryBase<User, Guid> users,
    ICurrentUser currentUser,
    ICompanyContext companyContext,
    AuthSessionService sessions) : IQueryHandler<GetSessionQuery, SessionInfo>
{
    public async Task<Result<SessionInfo>> Handle(GetSessionQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new InvalidRefreshTokenException();
        var user = await UserLoader.WithAccess(users).SingleOrDefaultAsync(u => u.Id == userId, cancellationToken)
                   ?? throw new InvalidRefreshTokenException();

        var companies = await sessions.GetCompaniesAsync(user, cancellationToken);
        var readable = companyContext.ReadableCompanyIds.Where(id => companies.Any(c => c.Id == id)).ToList();
        var permissions = await sessions.GetPermissionsAsync(user, readable, cancellationToken);
        var current = companies.FirstOrDefault(c => c.Id == companyContext.ActiveCompanyId);

        return new SessionInfo(new SessionUser(user.Id, user.Username, user.FullName), companies, current,
            companyContext.IsAllCompanies, await sessions.CanViewAllAsync(user, companies, cancellationToken),
            permissions.Order().ToList());
    }
}
