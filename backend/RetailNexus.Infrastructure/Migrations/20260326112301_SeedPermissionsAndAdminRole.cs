using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailNexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedPermissionsAndAdminRole : Migration
    {
        // Admin ロールの固定 UUID
        private const string AdminRoleId = "00000000-0000-0000-0000-000000000001";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. 権限マスタ ──
            var permissions = new (string Id, string Code, string Name, string Category)[]
            {
                ("10000000-0000-0000-0000-000000000001", "products.view",             "商品閲覧",           "商品管理"),
                ("10000000-0000-0000-0000-000000000002", "products.create",           "商品作成",           "商品管理"),
                ("10000000-0000-0000-0000-000000000003", "products.edit",             "商品編集",           "商品管理"),
                ("10000000-0000-0000-0000-000000000004", "products.delete",           "商品削除",           "商品管理"),
                ("10000000-0000-0000-0000-000000000011", "suppliers.view",            "仕入先閲覧",         "仕入先管理"),
                ("10000000-0000-0000-0000-000000000012", "suppliers.create",          "仕入先作成",         "仕入先管理"),
                ("10000000-0000-0000-0000-000000000013", "suppliers.edit",            "仕入先編集",         "仕入先管理"),
                ("10000000-0000-0000-0000-000000000014", "suppliers.delete",          "仕入先削除",         "仕入先管理"),
                ("10000000-0000-0000-0000-000000000021", "stores.view",              "店舗閲覧",           "店舗管理"),
                ("10000000-0000-0000-0000-000000000022", "stores.create",            "店舗作成",           "店舗管理"),
                ("10000000-0000-0000-0000-000000000023", "stores.edit",              "店舗編集",           "店舗管理"),
                ("10000000-0000-0000-0000-000000000024", "stores.delete",            "店舗削除",           "店舗管理"),
                ("10000000-0000-0000-0000-000000000031", "areas.view",               "エリア閲覧",         "エリア管理"),
                ("10000000-0000-0000-0000-000000000032", "areas.create",             "エリア作成",         "エリア管理"),
                ("10000000-0000-0000-0000-000000000033", "areas.edit",               "エリア編集",         "エリア管理"),
                ("10000000-0000-0000-0000-000000000034", "areas.delete",             "エリア削除",         "エリア管理"),
                ("10000000-0000-0000-0000-000000000041", "store-types.view",         "店舗種別閲覧",       "店舗種別管理"),
                ("10000000-0000-0000-0000-000000000042", "store-types.create",       "店舗種別作成",       "店舗種別管理"),
                ("10000000-0000-0000-0000-000000000043", "store-types.edit",         "店舗種別編集",       "店舗種別管理"),
                ("10000000-0000-0000-0000-000000000044", "store-types.delete",       "店舗種別削除",       "店舗種別管理"),
                ("10000000-0000-0000-0000-000000000051", "product-categories.view",  "商品カテゴリ閲覧",   "商品カテゴリ管理"),
                ("10000000-0000-0000-0000-000000000052", "product-categories.create","商品カテゴリ作成",   "商品カテゴリ管理"),
                ("10000000-0000-0000-0000-000000000053", "product-categories.edit",  "商品カテゴリ編集",   "商品カテゴリ管理"),
                ("10000000-0000-0000-0000-000000000054", "product-categories.delete","商品カテゴリ削除",   "商品カテゴリ管理"),
                ("10000000-0000-0000-0000-000000000061", "users.view",              "ユーザー閲覧",       "ユーザー管理"),
                ("10000000-0000-0000-0000-000000000062", "users.create",            "ユーザー作成",       "ユーザー管理"),
                ("10000000-0000-0000-0000-000000000063", "users.edit",              "ユーザー編集",       "ユーザー管理"),
                ("10000000-0000-0000-0000-000000000064", "users.delete",            "ユーザー削除",       "ユーザー管理"),
                ("10000000-0000-0000-0000-000000000071", "roles.view",              "ロール閲覧",         "ロール管理"),
                ("10000000-0000-0000-0000-000000000072", "roles.create",            "ロール作成",         "ロール管理"),
                ("10000000-0000-0000-0000-000000000073", "roles.edit",              "ロール編集",         "ロール管理"),
                ("10000000-0000-0000-0000-000000000074", "roles.delete",            "ロール削除",         "ロール管理"),
                ("10000000-0000-0000-0000-000000000081", "dashboard.view",          "ダッシュボード閲覧", "ダッシュボード"),
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

            // ── 2. Admin ロール ──
            migrationBuilder.Sql(
                $"""
                 INSERT INTO roles (role_id, role_name, description, is_active, created_at, updated_at)
                 VALUES ('{AdminRoleId}', 'Admin', '全権限を持つ管理者ロール', true, now(), now())
                 ON CONFLICT DO NOTHING;
                 """);

            // ── 3. Admin ロールに全権限を付与 ──
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
            migrationBuilder.Sql($"DELETE FROM role_permissions WHERE role_id = '{AdminRoleId}';");
            migrationBuilder.Sql($"DELETE FROM roles WHERE role_id = '{AdminRoleId}';");
            migrationBuilder.Sql("DELETE FROM permissions WHERE permission_id LIKE '10000000-0000-0000-0000-%';");
        }
    }
}
