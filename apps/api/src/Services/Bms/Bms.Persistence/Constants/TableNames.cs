namespace Bms.Persistence.Constants;

/// <summary>Tên schema và bảng — khai báo tập trung, không viết chuỗi rời rạc trong configuration.</summary>
public static class TableNames
{
    public const string SystemSchema = "system";
    public const string AuditLogs = "audit_logs";
    public const string RefreshTokens = "refresh_tokens";

    public const string AdminSchema = "admin";
    public const string Tenants = "tenants";
    public const string Companies = "companies";
    public const string Users = "users";
    public const string UserCompanyAccesses = "user_company_accesses";
    public const string UserRoleAssignments = "user_role_assignments";
    public const string Roles = "roles";
    public const string RolePermissions = "role_permissions";
    public const string RoleDataScopes = "role_data_scopes";
}
