using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailNexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAuditLogPermission : Migration
    {
        private const string PermissionId = "10000000-0000-0000-0000-000000000091";
        private const string AdminRoleId = "00000000-0000-0000-0000-000000000001";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"""
                 INSERT INTO permissions (permission_id, permission_code, permission_name, category)
                 VALUES ('{PermissionId}', 'auditlog.view', '監査ログ閲覧', '監査ログ')
                 ON CONFLICT (permission_code) DO NOTHING;
                 """);

            migrationBuilder.Sql(
                $"""
                 INSERT INTO role_permissions (role_id, permission_id)
                 VALUES ('{AdminRoleId}', '{PermissionId}')
                 ON CONFLICT DO NOTHING;
                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DELETE FROM role_permissions WHERE permission_id = '{PermissionId}';");
            migrationBuilder.Sql($"DELETE FROM permissions WHERE permission_id = '{PermissionId}';");
        }
    }
}
