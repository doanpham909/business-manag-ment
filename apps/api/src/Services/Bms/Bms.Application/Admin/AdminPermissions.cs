namespace Bms.Application.Admin;

/// <summary>Mã quyền module Quản trị hệ thống (khớp <see cref="Authorization.PermissionCatalog"/>).</summary>
public static class AdminPermissions
{
    public const string CompanyView = "admin.company.view";
    public const string CompanyCreate = "admin.company.create";
    public const string CompanyUpdate = "admin.company.update";
    public const string CompanyViewAll = "admin.company.view-all";

    public const string UserView = "admin.user.view";
    public const string UserCreate = "admin.user.create";
    public const string UserUpdate = "admin.user.update";
    public const string UserDelete = "admin.user.delete";

    public const string RoleView = "admin.role.view";
    public const string RoleCreate = "admin.role.create";
    public const string RoleUpdate = "admin.role.update";
    public const string RoleDelete = "admin.role.delete";

    public const string CatalogView = "admin.catalog.view";
    public const string CatalogCreate = "admin.catalog.create";
    public const string CatalogUpdate = "admin.catalog.update";
    public const string CatalogDelete = "admin.catalog.delete";

    public const string AuditLogView = "admin.auditlog.view";
}
