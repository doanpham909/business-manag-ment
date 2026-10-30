using Bms.Domain.Common.Rules;
using BuildingBlocks.Contracts.Domain.DDD;
using BuildingBlocks.Contracts.Domain.Entities;
using BuildingBlocks.Contracts.Domain.Exceptions;

namespace Bms.Domain.Admin.Roles;

/// <summary>
/// Vai trò do tenant tự định nghĩa: tập quyền (mã permission) và phạm vi dữ liệu theo từng phân hệ.
/// Vai trò được gán cho người dùng theo từng công ty (xem User.RoleAssignments).
/// </summary>
public sealed class Role : EventEntityAuditBase<Guid>, IAggregateRoot, ITenantEntity
{
    private readonly List<RolePermission> _permissions = [];
    private readonly List<RoleDataScope> _dataScopes = [];

    public Guid TenantId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    /// <summary>Vai trò quản trị khách hàng: luôn có mọi quyền, không sửa/xóa được.</summary>
    public bool IsSystem { get; private set; }

    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();
    public IReadOnlyCollection<RoleDataScope> DataScopes => _dataScopes.AsReadOnly();

    private Role() { }

    public static Role Create(string code, string name, string? description, bool isSystem = false,
        Guid tenantId = default)
    {
        CheckRule(new TextMustNotBeEmptyRule(code, nameof(Code), "Mã vai trò"));
        var role = new Role { Id = Guid.CreateVersion7(), TenantId = tenantId, Code = code.Trim(), IsSystem = isSystem };
        role.Update(name, description);
        return role;
    }

    public void Update(string name, string? description)
    {
        CheckRule(new TextMustNotBeEmptyRule(name, nameof(Name), "Tên vai trò"));
        Name = name.Trim();
        Description = description?.Trim();
    }

    /// <summary>Thay toàn bộ quyền (dòng cũ bị xóa theo quan hệ bắt buộc, dòng mới thêm qua navigation).</summary>
    public void SetPermissions(IEnumerable<string> codes)
    {
        _permissions.Clear();
        _permissions.AddRange(codes.Distinct().Select(code => RolePermission.Create(Id, code)));
    }

    public void SetDataScopes(IEnumerable<(string Module, DataScope Scope)> scopes)
    {
        _dataScopes.Clear();
        _dataScopes.AddRange(scopes.DistinctBy(s => s.Module).Select(s => RoleDataScope.Create(Id, s.Module, s.Scope)));
    }
}

public sealed class RolePermission : EntityBase<Guid>
{
    public Guid RoleId { get; private set; }
    public string PermissionCode { get; private set; } = string.Empty;

    private RolePermission() { }

    internal static RolePermission Create(Guid roleId, string code) =>
        new() { Id = Guid.CreateVersion7(), RoleId = roleId, PermissionCode = code };
}

public sealed class RoleDataScope : EntityBase<Guid>
{
    public Guid RoleId { get; private set; }
    public string Module { get; private set; } = string.Empty;
    public DataScope Scope { get; private set; }

    private RoleDataScope() { }

    internal static RoleDataScope Create(Guid roleId, string module, DataScope scope) =>
        new() { Id = Guid.CreateVersion7(), RoleId = roleId, Module = module, Scope = scope };
}

/// <summary>Phạm vi dữ liệu trong công ty hiện hành (multi-tenancy-and-security.md).</summary>
public enum DataScope
{
    Own,
    Project,
    Department,
    DepartmentTree,
    All,
}

public sealed class RoleNotFoundException(Guid id) : NotFoundException($"Không tìm thấy vai trò (Id = {id}).");

public sealed class RoleCodeAlreadyExistsException(string code)
    : ConflictException($"Mã vai trò \"{code}\" đã tồn tại.");

public sealed class RoleIsSystemException()
    : ConflictException("Không được sửa quyền hoặc xóa vai trò quản trị hệ thống.");

public sealed class RoleInUseException()
    : ConflictException("Vai trò đang được gán cho người dùng, không xóa được.");
