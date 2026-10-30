using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bms.Persistence.Migrations;

/// <inheritdoc />
public partial class AddCatalogsDepartmentsProjects : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "hrm");

        migrationBuilder.EnsureSchema(
            name: "projects");

        migrationBuilder.AddColumn<uint>(
            name: "xmin",
            schema: "admin",
            table: "users",
            type: "xid",
            rowVersion: true,
            nullable: false,
            defaultValue: 0u);

        migrationBuilder.AddColumn<uint>(
            name: "xmin",
            schema: "admin",
            table: "roles",
            type: "xid",
            rowVersion: true,
            nullable: false,
            defaultValue: 0u);

        migrationBuilder.AddColumn<uint>(
            name: "xmin",
            schema: "admin",
            table: "companies",
            type: "xid",
            rowVersion: true,
            nullable: false,
            defaultValue: 0u);

        migrationBuilder.CreateTable(
            name: "catalog_items",
            schema: "admin",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                sort_order = table.Column<int>(type: "integer", nullable: false),
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
                table.PrimaryKey("pk_catalog_items", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "departments",
            schema: "hrm",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                company_id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                type_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                sort_order = table.Column<int>(type: "integer", nullable: false),
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
                table.PrimaryKey("pk_departments", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "document_sequences",
            schema: "system",
            columns: table => new
            {
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                company_id = table.Column<Guid>(type: "uuid", nullable: false),
                document_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                period = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                last_number = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_document_sequences", x => new { x.tenant_id, x.company_id, x.document_type, x.period });
            });

        migrationBuilder.CreateTable(
            name: "projects",
            schema: "projects",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                company_id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                parent_project_id = table.Column<Guid>(type: "uuid", nullable: true),
                project_type_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                construction_type_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                department_id = table.Column<Guid>(type: "uuid", nullable: true),
                manager_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                start_date = table.Column<DateOnly>(type: "date", nullable: true),
                end_date = table.Column<DateOnly>(type: "date", nullable: true),
                description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                table.PrimaryKey("pk_projects", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_catalog_items_tenant_id",
            schema: "admin",
            table: "catalog_items",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ix_catalog_items_tenant_id_type_code",
            schema: "admin",
            table: "catalog_items",
            columns: new[] { "tenant_id", "type", "code" },
            unique: true,
            filter: "is_deleted = false");

        migrationBuilder.CreateIndex(
            name: "ix_departments_tenant_id",
            schema: "hrm",
            table: "departments",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ix_departments_tenant_id_company_id",
            schema: "hrm",
            table: "departments",
            columns: new[] { "tenant_id", "company_id" });

        migrationBuilder.CreateIndex(
            name: "ix_departments_tenant_id_company_id_code",
            schema: "hrm",
            table: "departments",
            columns: new[] { "tenant_id", "company_id", "code" },
            unique: true,
            filter: "is_deleted = false");

        migrationBuilder.CreateIndex(
            name: "ix_departments_tenant_id_company_id_parent_id",
            schema: "hrm",
            table: "departments",
            columns: new[] { "tenant_id", "company_id", "parent_id" });

        migrationBuilder.CreateIndex(
            name: "ix_projects_tenant_id",
            schema: "projects",
            table: "projects",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ix_projects_tenant_id_company_id",
            schema: "projects",
            table: "projects",
            columns: new[] { "tenant_id", "company_id" });

        migrationBuilder.CreateIndex(
            name: "ix_projects_tenant_id_company_id_code",
            schema: "projects",
            table: "projects",
            columns: new[] { "tenant_id", "company_id", "code" },
            unique: true,
            filter: "is_deleted = false");

        migrationBuilder.CreateIndex(
            name: "ix_projects_tenant_id_company_id_parent_project_id",
            schema: "projects",
            table: "projects",
            columns: new[] { "tenant_id", "company_id", "parent_project_id" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "catalog_items",
            schema: "admin");

        migrationBuilder.DropTable(
            name: "departments",
            schema: "hrm");

        migrationBuilder.DropTable(
            name: "document_sequences",
            schema: "system");

        migrationBuilder.DropTable(
            name: "projects",
            schema: "projects");

        migrationBuilder.DropColumn(
            name: "xmin",
            schema: "admin",
            table: "users");

        migrationBuilder.DropColumn(
            name: "xmin",
            schema: "admin",
            table: "roles");

        migrationBuilder.DropColumn(
            name: "xmin",
            schema: "admin",
            table: "companies");
    }
}
