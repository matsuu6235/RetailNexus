using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailNexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedPurchaseAndStoreRequestPermissions : Migration
    {
        private const string AdminRoleId = "00000000-0000-0000-0000-000000000001";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var permissions = new (string Id, string Code, string Name, string Category)[]
            {
                ("10000000-0000-0000-0000-000000000101", "purchases.view",            "発注閲覧",       "発注管理"),
                ("10000000-0000-0000-0000-000000000102", "purchases.create",          "発注作成",       "発注管理"),
                ("10000000-0000-0000-0000-000000000103", "purchases.edit",            "発注編集",       "発注管理"),
                ("10000000-0000-0000-0000-000000000104", "purchases.delete",          "発注削除",       "発注管理"),
                ("10000000-0000-0000-0000-000000000105", "purchases.approve",         "発注承認",       "発注管理"),
                ("10000000-0000-0000-0000-000000000111", "store-requests.view",       "発送依頼閲覧",   "発送依頼"),
                ("10000000-0000-0000-0000-000000000112", "store-requests.create",     "発送依頼作成",   "発送依頼"),
                ("10000000-0000-0000-0000-000000000113", "store-requests.edit",       "発送依頼編集",   "発送依頼"),
                ("10000000-0000-0000-0000-000000000114", "store-requests.delete",     "発送依頼削除",   "発送依頼"),
                ("10000000-0000-0000-0000-000000000115", "store-requests.approve",    "発送依頼承認",   "発送依頼"),
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

            // Admin ロールに全権限を付与
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
                "10000000-0000-0000-0000-000000000101",
                "10000000-0000-0000-0000-000000000102",
                "10000000-0000-0000-0000-000000000103",
                "10000000-0000-0000-0000-000000000104",
                "10000000-0000-0000-0000-000000000105",
                "10000000-0000-0000-0000-000000000111",
                "10000000-0000-0000-0000-000000000112",
                "10000000-0000-0000-0000-000000000113",
                "10000000-0000-0000-0000-000000000114",
                "10000000-0000-0000-0000-000000000115",
            };

            foreach (var id in permissionIds)
            {
                migrationBuilder.Sql($"DELETE FROM role_permissions WHERE permission_id = '{id}';");
                migrationBuilder.Sql($"DELETE FROM permissions WHERE permission_id = '{id}';");
            }
        }
    }
}
