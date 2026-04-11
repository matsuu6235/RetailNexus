using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailNexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInventoryPermissions : Migration
    {
        private const string AdminRoleId = "00000000-0000-0000-0000-000000000001";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var permissions = new (string Id, string Code, string Name, string Category)[]
            {
                ("10000000-0000-0000-0000-000000000201", "inventory.view",    "在庫閲覧",       "在庫管理"),
                ("10000000-0000-0000-0000-000000000202", "inventory.edit",    "在庫入出庫",     "在庫管理"),
                ("10000000-0000-0000-0000-000000000203", "inventory.reorder", "発注点設定",     "在庫管理"),
            };

            foreach (var (id, code, name, category) in permissions)
            {
                migrationBuilder.Sql(
                    $"""
                     INSERT INTO permissions (permission_id, permission_code, permission_name, category)
                     VALUES ('{id}', '{code}', '{name}', '{category}')
                     ON CONFLICT (permission_code) DO NOTHING;
                     """);
            }

            foreach (var (id, _, _, _) in permissions)
            {
                migrationBuilder.Sql(
                    $"""
                     INSERT INTO role_permissions (role_id, permission_id)
                     VALUES ('{AdminRoleId}', '{id}')
                     ON CONFLICT DO NOTHING;
                     """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var permissionIds = new[]
            {
                "10000000-0000-0000-0000-000000000201",
                "10000000-0000-0000-0000-000000000202",
                "10000000-0000-0000-0000-000000000203",
            };

            foreach (var id in permissionIds)
            {
                migrationBuilder.Sql($"DELETE FROM role_permissions WHERE permission_id = '{id}';");
                migrationBuilder.Sql($"DELETE FROM permissions WHERE permission_id = '{id}';");
            }
        }
    }
}
