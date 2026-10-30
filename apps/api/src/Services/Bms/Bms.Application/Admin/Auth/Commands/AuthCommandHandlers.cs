using Bms.Domain.Admin.Auth;
using Bms.Domain.Admin.Users;
using BuildingBlocks.Contracts.Abstractions;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Contracts.Persistence;
using BuildingBlocks.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Bms.Application.Admin.Auth.Commands;

/// <summary>Tải người dùng kèm công ty được vào và vai trò (bỏ filter tenant vì chưa có tenant trong ngữ cảnh).</summary>
internal static class UserLoader
{
    public static IQueryable<User> WithAccess(IRepositoryBase<User, Guid> users) =>
        users.FindAll(null, u => u.CompanyAccesses, u => u.RoleAssignments)
            .IgnoreQueryFilters([QueryFilterNames.Tenant]);
}

internal sealed class LoginCommandHandler(
    IRepositoryBase<User, Guid> users, IPasswordHasher hasher, AuthSessionService sessions)
    : ICommandHandler<LoginCommand, AuthSession>
{
    public async Task<Result<AuthSession>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim().ToLowerInvariant();
        var user = await UserLoader.WithAccess(users).SingleOrDefaultAsync(u => u.Username == username, cancellationToken);

        if (user is null || !hasher.Verify(user.PasswordHash, request.Password))
            throw new InvalidCredentialsException();
        if (user.Status == UserStatus.Locked)
            throw new UserLockedException();

        var companies = await sessions.GetCompaniesAsync(user, cancellationToken);
        // Chỉ được vào một công ty → chọn sẵn để khỏi thêm một bước
        Guid? companyId = companies.Count == 1 ? companies[0].Id : null;
        return await sessions.IssueAsync(user, companyId, false, companies, cancellationToken);
    }
}

internal sealed class SelectCompanyCommandHandler(
    IRepositoryBase<User, Guid> users,
    IRepositoryBase<RefreshToken, Guid> refreshTokens,
    ICurrentUser currentUser,
    IAccessTokenIssuer tokenIssuer,
    AuthSessionService sessions,
    TimeProvider timeProvider) : ICommandHandler<SelectCompanyCommand, AuthSession>
{
    public async Task<Result<AuthSession>> Handle(SelectCompanyCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new InvalidRefreshTokenException();
        var user = await UserLoader.WithAccess(users).SingleOrDefaultAsync(u => u.Id == userId, cancellationToken)
                   ?? throw new InvalidRefreshTokenException();
        if (user.Status == UserStatus.Locked)
            throw new UserLockedException();

        var companies = await sessions.GetCompaniesAsync(user, cancellationToken);
        if (request.AllCompanies)
        {
            if (!await sessions.CanViewAllAsync(user, companies, cancellationToken))
                throw new AllCompaniesAccessDeniedException();
        }
        else if (companies.All(c => c.Id != request.CompanyId))
        {
            throw new CompanyAccessDeniedException();
        }

        if (!string.IsNullOrEmpty(request.CurrentRefreshToken))
        {
            var hash = tokenIssuer.HashRefreshToken(request.CurrentRefreshToken);
            var old = await refreshTokens.FindSingleAsync(t => t.TokenHash == hash && t.UserId == userId, cancellationToken);
            old?.Revoke(timeProvider.GetUtcNow());
        }

        return await sessions.IssueAsync(user, request.CompanyId, request.AllCompanies, companies, cancellationToken);
    }
}

internal sealed class RefreshSessionCommandHandler(
    IRepositoryBase<User, Guid> users,
    IRepositoryBase<RefreshToken, Guid> refreshTokens,
    IAccessTokenIssuer tokenIssuer,
    AuthSessionService sessions,
    TimeProvider timeProvider) : ICommandHandler<RefreshSessionCommand, AuthSession>
{
    public async Task<Result<AuthSession>> Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var hash = tokenIssuer.HashRefreshToken(request.RefreshToken);
        var token = await refreshTokens.FindSingleAsync(t => t.TokenHash == hash, cancellationToken);
        if (token is null || !token.IsActive(now))
            throw new InvalidRefreshTokenException();

        var user = await UserLoader.WithAccess(users).SingleOrDefaultAsync(u => u.Id == token.UserId, cancellationToken);
        if (user is null || user.Status == UserStatus.Locked)
            throw new InvalidRefreshTokenException();

        token.Revoke(now);
        var companies = await sessions.GetCompaniesAsync(user, cancellationToken);
        // Công ty cũ không còn được vào (bị gỡ / ngừng hoạt động) → về trạng thái chưa chọn công ty
        var companyId = token.CompanyId is { } id && companies.Any(c => c.Id == id) ? id : (Guid?)null;
        var allCompanies = token.AllCompanies && await sessions.CanViewAllAsync(user, companies, cancellationToken);
        return await sessions.IssueAsync(user, companyId, allCompanies, companies, cancellationToken);
    }
}

internal sealed class LogoutCommandHandler(
    IRepositoryBase<RefreshToken, Guid> refreshTokens, IAccessTokenIssuer tokenIssuer, TimeProvider timeProvider)
    : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
            return Result.Success();

        var hash = tokenIssuer.HashRefreshToken(request.RefreshToken);
        var token = await refreshTokens.FindSingleAsync(t => t.TokenHash == hash, cancellationToken);
        token?.Revoke(timeProvider.GetUtcNow());
        return Result.Success();
    }
}
