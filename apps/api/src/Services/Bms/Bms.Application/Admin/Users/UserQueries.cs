using Bms.Application.Common.Paging;
using Bms.Domain.Admin.Users;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Contracts.Persistence;
using BuildingBlocks.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Bms.Application.Admin.Users;

public sealed record UserListItem(
    Guid Id, string Username, string FullName, string? Email, string? Phone, UserStatus Status, int CompanyCount);

/// <summary>Công ty người dùng được vào và các vai trò tại công ty đó.</summary>
public sealed record UserCompanyAssignment(Guid CompanyId, IReadOnlyList<Guid> RoleIds);

public sealed record UserDetail(
    Guid Id, string Username, string FullName, string? Email, string? Phone, UserStatus Status, Guid? EmployeeId,
    IReadOnlyList<UserCompanyAssignment> Assignments, uint Version);

public sealed record GetUsersQuery(int PageIndex = 1, int PageSize = 20, string? SearchTerm = null, UserStatus? Status = null)
    : IQuery<PagedResult<UserListItem>>;

public sealed record GetUserQuery(Guid Id) : IQuery<UserDetail>;

internal sealed class UserQueryHandlers(IRepositoryBase<User, Guid> users)
    : IQueryHandler<GetUsersQuery, PagedResult<UserListItem>>, IQueryHandler<GetUserQuery, UserDetail>
{
    public async Task<Result<PagedResult<UserListItem>>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var query = users.FindAll();
        if (request.Status.HasValue)
            query = query.Where(u => u.Status == request.Status);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = $"%{request.SearchTerm.Trim()}%";
            query = query.Where(u => EF.Functions.ILike(u.Username, term) || EF.Functions.ILike(u.FullName, term)
                                     || (u.Email != null && EF.Functions.ILike(u.Email, term)));
        }

        return await query.OrderBy(u => u.Username)
            .Select(u => new UserListItem(u.Id, u.Username, u.FullName, u.Email, u.Phone, u.Status, u.CompanyAccesses.Count))
            .ToPagedResultAsync(request.PageIndex, request.PageSize, ct);
    }

    public async Task<Result<UserDetail>> Handle(GetUserQuery request, CancellationToken ct)
    {
        var user = await users.FindAll(u => u.Id == request.Id, u => u.CompanyAccesses, u => u.RoleAssignments)
                       .SingleOrDefaultAsync(ct) ?? throw new UserNotFoundException(request.Id);

        var assignments = user.CompanyAccesses
            .Select(a => new UserCompanyAssignment(a.CompanyId,
                user.RoleAssignments.Where(r => r.CompanyId == a.CompanyId).Select(r => r.RoleId).ToList()))
            .ToList();
        return new UserDetail(user.Id, user.Username, user.FullName, user.Email, user.Phone, user.Status,
            user.EmployeeId, assignments, user.Version);
    }
}
