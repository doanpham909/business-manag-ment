using Bms.Application.Authorization;
using Bms.Domain.Admin.Auth;
using Bms.Domain.Admin.Roles;
using Bms.Domain.Admin.Users;
using BuildingBlocks.Contracts.Abstractions;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Contracts.Persistence;
using BuildingBlocks.Shared.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Bms.Application.Admin.Users;

public sealed record CreateUserCommand(
    string Username, string FullName, string? Email, string? Phone, string Password,
    IReadOnlyList<UserCompanyAssignment> Assignments) : ICommand<Guid>;

public sealed record UpdateUserCommand(
    Guid Id, string FullName, string? Email, string? Phone,
    IReadOnlyList<UserCompanyAssignment> Assignments, uint? Version) : ICommand;

public sealed record SetUserLockedCommand(Guid Id, bool Locked) : ICommand;

public sealed record ResetUserPasswordCommand(Guid Id, string NewPassword) : ICommand;

/// <summary>Bảo vệ quản trị cuối cùng của khách hàng (BR-SEC-01) và thu hồi phiên khi khóa (BR-SEC-07).</summary>
internal sealed class UserAdminGuard(
    IRepositoryBase<User, Guid> users, IRepositoryBase<Role, Guid> roles, IRepositoryBase<RefreshToken, Guid> tokens,
    TimeProvider timeProvider)
{
    public async Task EnsureNotLastAdminAsync(User user, CancellationToken ct)
    {
        var systemRoleIds = await roles.FindAll(r => r.IsSystem).Select(r => r.Id).ToListAsync(ct);
        if (!user.RoleAssignments.Any(a => systemRoleIds.Contains(a.RoleId)))
            return;

        var otherAdmins = await users.FindAll(u => u.Id != user.Id && u.Status == UserStatus.Active)
            .AnyAsync(u => u.RoleAssignments.Any(a => systemRoleIds.Contains(a.RoleId)), ct);
        if (!otherAdmins)
            throw new LastTenantAdminException();
    }

    public async Task RevokeSessionsAsync(Guid userId, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow();
        var active = await tokens.FindAllTracking(t => t.UserId == userId && t.RevokedAt == null).ToListAsync(ct);
        active.ForEach(t => t.Revoke(now));
    }
}

internal sealed class CreateUserCommandHandler(IRepositoryBase<User, Guid> users, IPasswordHasher hasher)
    : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var username = request.Username.Trim().ToLowerInvariant();
        // Tên đăng nhập duy nhất toàn nền tảng → kiểm tra bỏ qua filter tenant (ADR-0002)
        if (await users.FindAll().IgnoreQueryFilters([QueryFilterNames.Tenant]).AnyAsync(u => u.Username == username, ct))
            throw new UsernameAlreadyExistsException(username);

        var user = User.Create(username, request.FullName, request.Email, request.Phone, hasher.Hash(request.Password));
        ApplyAssignments(user, request.Assignments);
        await users.AddAsync(user, ct);
        return user.Id;
    }

    internal static void ApplyAssignments(User user, IReadOnlyList<UserCompanyAssignment> assignments)
    {
        user.SetCompanyAccess(assignments.Select(a => a.CompanyId));
        foreach (var assignment in assignments)
            user.SetRoles(assignment.CompanyId, assignment.RoleIds);
    }
}

internal sealed class UpdateUserCommandHandler(IRepositoryBase<User, Guid> users, IRepositoryBase<Role, Guid> roles)
    : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(request.Id, ct, u => u.CompanyAccesses, u => u.RoleAssignments)
                   ?? throw new UserNotFoundException(request.Id);
        user.EnsureVersion(request.Version);

        // Không được gỡ vai trò quản trị của quản trị cuối cùng
        var systemRoleIds = await roles.FindAll(r => r.IsSystem).Select(r => r.Id).ToListAsync(ct);
        var wasAdmin = user.RoleAssignments.Any(a => systemRoleIds.Contains(a.RoleId));
        var staysAdmin = request.Assignments.Any(a => a.RoleIds.Any(systemRoleIds.Contains));
        if (wasAdmin && !staysAdmin)
        {
            var others = await users.FindAll(u => u.Id != user.Id && u.Status == UserStatus.Active)
                .AnyAsync(u => u.RoleAssignments.Any(a => systemRoleIds.Contains(a.RoleId)), ct);
            if (!others)
                throw new LastTenantAdminException();
        }

        user.Update(request.FullName, request.Email, request.Phone);
        CreateUserCommandHandler.ApplyAssignments(user, request.Assignments);
        return Result.Success();
    }
}

internal sealed class SetUserLockedCommandHandler(IRepositoryBase<User, Guid> users, UserAdminGuard guard)
    : ICommandHandler<SetUserLockedCommand>
{
    public async Task<Result> Handle(SetUserLockedCommand request, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(request.Id, ct, u => u.RoleAssignments)
                   ?? throw new UserNotFoundException(request.Id);

        if (request.Locked)
        {
            await guard.EnsureNotLastAdminAsync(user, ct);
            user.Lock();
            await guard.RevokeSessionsAsync(user.Id, ct);
        }
        else
        {
            user.Unlock();
        }

        return Result.Success();
    }
}

internal sealed class ResetUserPasswordCommandHandler(
    IRepositoryBase<User, Guid> users, IPasswordHasher hasher, UserAdminGuard guard)
    : ICommandHandler<ResetUserPasswordCommand>
{
    public async Task<Result> Handle(ResetUserPasswordCommand request, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(request.Id, ct) ?? throw new UserNotFoundException(request.Id);
        user.ChangePasswordHash(hasher.Hash(request.NewPassword));
        await guard.RevokeSessionsAsync(user.Id, ct);
        return Result.Success();
    }
}

internal static class PasswordRules
{
    public const int MinLength = 8;

    public static void Apply<T>(IRuleBuilder<T, string> rule) => rule
        .NotEmpty().WithMessage("Vui lòng nhập mật khẩu.")
        .MinimumLength(MinLength).WithMessage($"Mật khẩu tối thiểu {MinLength} ký tự.")
        .MaximumLength(200);
}

internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Vui lòng nhập tên đăng nhập.").MaximumLength(100);
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Vui lòng nhập họ tên.").MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().WithMessage("Email không đúng định dạng.").MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20);
        PasswordRules.Apply(RuleFor(x => x.Password));
    }
}

internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Vui lòng nhập họ tên.").MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().WithMessage("Email không đúng định dạng.").MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20);
    }
}

internal sealed class ResetUserPasswordCommandValidator : AbstractValidator<ResetUserPasswordCommand>
{
    public ResetUserPasswordCommandValidator() => PasswordRules.Apply(RuleFor(x => x.NewPassword));
}
