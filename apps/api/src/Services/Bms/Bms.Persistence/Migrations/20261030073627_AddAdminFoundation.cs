using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bms.Persistence.Migrations;

/// <inheritdoc />
public partial class AddAdminFoundation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "system");

        migrationBuilder.EnsureSchema(
            name: "admin");

        migrationBuilder.CreateTable(
            name: "audit_logs",
            schema: "system",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                company_id = table.Column<Guid>(type: "uuid", nullable: true),
                user_id = table.Column<Guid>(type: "uuid", nullable: true),
                entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                entity_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                changes = table.Column<string>(type: "jsonb", nullable: false),
                occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_audit_logs", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "companies",
            schema: "admin",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                tax_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_companies", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "refresh_tokens",
            schema: "system",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                company_id = table.Column<Guid>(type: "uuid", nullable: true),
                all_companies = table.Column<bool>(type: "boolean", nullable: false),
                token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_refresh_tokens", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "roles",
            schema: "admin",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                is_system = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_roles", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "tenants",
            schema: "admin",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tenants", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "users",
            schema: "admin",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "role_data_scopes",
            schema: "admin",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                role_id = table.Column<Guid>(type: "uuid", nullable: false),
                module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                scope = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_role_data_scopes", x => x.id);
                table.ForeignKey(
                    name: "fk_role_data_scopes_roles_role_id",
                    column: x => x.role_id,
                    principalSchema: "admin",
                    principalTable: "roles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "role_permissions",
            schema: "admin",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                role_id = table.Column<Guid>(type: "uuid", nullable: false),
                permission_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_role_permissions", x => x.id);
                table.ForeignKey(
                    name: "fk_role_permissions_roles_role_id",
                    column: x => x.role_id,
                    principalSchema: "admin",
                    principalTable: "roles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_company_accesses",
            schema: "admin",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                company_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_company_accesses", x => x.id);
                table.ForeignKey(
                    name: "fk_user_company_accesses_user_user_id",
                    column: x => x.user_id,
                    principalSchema: "admin",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_role_assignments",
            schema: "admin",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                company_id = table.Column<Guid>(type: "uuid", nullable: false),
                role_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_role_assignments", x => x.id);
                table.ForeignKey(
                    name: "fk_user_role_assignments_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "admin",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_tenant_id",
            schema: "system",
            table: "audit_logs",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_tenant_id_entity_type_entity_id",
            schema: "system",
            table: "audit_logs",
            columns: new[] { "tenant_id", "entity_type", "entity_id" });

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_tenant_id_occurred_at",
            schema: "system",
            table: "audit_logs",
            columns: new[] { "tenant_id", "occurred_at" });

        migrationBuilder.CreateIndex(
            name: "ix_companies_tenant_id",
            schema: "admin",
            table: "companies",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ix_companies_tenant_id_code",
            schema: "admin",
            table: "companies",
            columns: new[] { "tenant_id", "code" },
            unique: true,
            filter: "is_deleted = false");

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_token_hash",
            schema: "system",
            table: "refresh_tokens",
            column: "token_hash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_user_id",
            schema: "system",
            table: "refresh_tokens",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_role_data_scopes_role_id_module",
            schema: "admin",
            table: "role_data_scopes",
            columns: new[] { "role_id", "module" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_role_permissions_role_id_permission_code",
            schema: "admin",
            table: "role_permissions",
            columns: new[] { "role_id", "permission_code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_roles_tenant_id",
            schema: "admin",
            table: "roles",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ix_roles_tenant_id_code",
            schema: "admin",
            table: "roles",
            columns: new[] { "tenant_id", "code" },
            unique: true,
            filter: "is_deleted = false");

        migrationBuilder.CreateIndex(
            name: "ix_tenants_code",
            schema: "admin",
            table: "tenants",
            column: "code",
            unique: true,
            filter: "is_deleted = false");

        migrationBuilder.CreateIndex(
            name: "ix_user_company_accesses_user_id_company_id",
            schema: "admin",
            table: "user_company_accesses",
            columns: new[] { "user_id", "company_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_user_role_assignments_role_id",
            schema: "admin",
            table: "user_role_assignments",
            column: "role_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_role_assignments_user_id_company_id_role_id",
            schema: "admin",
            table: "user_role_assignments",
            columns: new[] { "user_id", "company_id", "role_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_users_tenant_id",
            schema: "admin",
            table: "users",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ix_users_tenant_id_employee_id",
            schema: "admin",
            table: "users",
            columns: new[] { "tenant_id", "employee_id" });

        migrationBuilder.CreateIndex(
            name: "ix_users_username",
            schema: "admin",
            table: "users",
            column: "username",
            unique: true,
            filter: "is_deleted = false");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "audit_logs",
            schema: "system");

        migrationBuilder.DropTable(
            name: "companies",
            schema: "admin");

        migrationBuilder.DropTable(
            name: "refresh_tokens",
            schema: "system");

        migrationBuilder.DropTable(
            name: "role_data_scopes",
            schema: "admin");

        migrationBuilder.DropTable(
            name: "role_permissions",
            schema: "admin");

        migrationBuilder.DropTable(
            name: "tenants",
            schema: "admin");

        migrationBuilder.DropTable(
            name: "user_company_accesses",
            schema: "admin");

        migrationBuilder.DropTable(
            name: "user_role_assignments",
            schema: "admin");

        migrationBuilder.DropTable(
            name: "roles",
            schema: "admin");

        migrationBuilder.DropTable(
            name: "users",
            schema: "admin");
    }
}
