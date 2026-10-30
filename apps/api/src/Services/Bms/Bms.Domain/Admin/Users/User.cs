using Bms.Domain.Common.Rules;
using BuildingBlocks.Contracts.Domain.DDD;
using BuildingBlocks.Contracts.Domain.Entities;
using BuildingBlocks.Contracts.Persistence;

namespace Bms.Domain.Admin.Users;

/// <summary>
/// Tài khoản người dùng của tenant. Tên đăng nhập duy nhất toàn nền tảng (đăng nhập không nhập mã công ty).
/// Người dùng vào được các công ty trong <see cref="CompanyAccesses"/>; vai trò gán theo từng công ty.
/// </summary>
public sealed class User : EventEntityAuditBase<Guid>, IAggregateRoot, ITenantEntity, IVersioned
{
    public uint Version { get; private set; }

    private readonly List<UserCompanyAccess> _companyAccesses = [];
    private readonly List<UserRoleAssignment> _roleAssignments = [];

    public Guid TenantId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }

    [Sensitive]
    public string PasswordHash { get; private set; } = string.Empty;

    public UserStatus Status { get; private set; }

    /// <summary>Nhân viên liên kết (module Nhân sự). Tối đa một.</summary>
    public Guid? EmployeeId { get; private set; }

    public IReadOnlyCollection<UserCompanyAccess> CompanyAccesses => _companyAccesses.AsReadOnly();
    public IReadOnlyCollection<UserRoleAssignment> RoleAssignments => _roleAssignments.AsReadOnly();

    private User() { }

    public static User Create(string username, string fullName, string? email, string? phone, string passwordHash,
        Guid tenantId = default)
    {
        CheckRule(new TextMustNotBeEmptyRule(username, nameof(Username), "Tên đăng nhập"));
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Username = username.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Status = UserStatus.Active,
        };
        user.Update(fullName, email, phone);
        return user;
    }

    public void Update(string fullName, string? email, string? phone)
    {
        CheckRule(new TextMustNotBeEmptyRule(fullName, nameof(FullName), "Họ tên"));
        FullName = fullName.Trim();
        Email = email?.Trim();
        Phone = phone?.Trim();
    }

    public void ChangePasswordHash(string passwordHash) => PasswordHash = passwordHash;

    public void Lock() => Status = UserStatus.Locked;

    public void Unlock() => Status = UserStatus.Active;

    public void LinkEmployee(Guid? employeeId) => EmployeeId = employeeId;

    /// <summary>Thay danh sách công ty được vào; gỡ luôn vai trò ở công ty không còn quyền vào.</summary>
    public void SetCompanyAccess(IEnumerable<Guid> companyIds)
    {
        var ids = companyIds.Distinct().ToHashSet();
        _companyAccesses.RemoveAll(a => !ids.Contains(a.CompanyId));
        _roleAssignments.RemoveAll(r => !ids.Contains(r.CompanyId));
        foreach (var id in ids.Where(id => _companyAccesses.All(a => a.CompanyId != id)))
            _companyAccesses.Add(UserCompanyAccess.Create(Id, id));
    }

    /// <summary>Thay vai trò của người dùng tại một công ty (công ty phải nằm trong danh sách được vào).</summary>
    public void SetRoles(Guid companyId, IEnumerable<Guid> roleIds)
    {
        CheckRule(new UserMustHaveCompanyAccessRule(this, companyId));
        _roleAssignments.RemoveAll(r => r.CompanyId == companyId);
        _roleAssignments.AddRange(roleIds.Distinct().Select(roleId => UserRoleAssignment.Create(Id, companyId, roleId)));
    }

    public bool CanAccess(Guid companyId) => _companyAccesses.Any(a => a.CompanyId == companyId);
}

public sealed class UserCompanyAccess : EntityBase<Guid>
{
    public Guid UserId { get; private set; }
    public Guid CompanyId { get; private set; }

    private UserCompanyAccess() { }

    internal static UserCompanyAccess Create(Guid userId, Guid companyId) =>
        new() { Id = Guid.CreateVersion7(), UserId = userId, CompanyId = companyId };
}

public sealed class UserRoleAssignment : EntityBase<Guid>
{
    public Guid UserId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid RoleId { get; private set; }

    private UserRoleAssignment() { }

    internal static UserRoleAssignment Create(Guid userId, Guid companyId, Guid roleId) =>
        new() { Id = Guid.CreateVersion7(), UserId = userId, CompanyId = companyId, RoleId = roleId };
}

public enum UserStatus
{
    Active,
    Locked,
}

public sealed class UserMustHaveCompanyAccessRule(User user, Guid companyId) : IBusinessRule
{
    public bool IsBroken() => !user.CanAccess(companyId);

    public string Message => "Người dùng chưa được cấp quyền vào công ty này.";

    public string FieldName => "CompanyId";
}
