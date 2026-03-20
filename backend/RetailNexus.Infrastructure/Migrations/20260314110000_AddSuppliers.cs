using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailNexus.Infrastructure.Migrations
{
    public partial class AddSuppliers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pgcrypto;");

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    supplier_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    supplier_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.supplier_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_is_active",
                table: "suppliers",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_supplier_code",
                table: "suppliers",
                column: "supplier_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_supplier_name",
                table: "suppliers",
                column: "supplier_name");

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND table_name = 'users'
                          AND column_name = 'user_id'
                    ) THEN
                        ALTER TABLE suppliers
                            ADD CONSTRAINT fk_suppliers_created_by
                            FOREIGN KEY (created_by) REFERENCES users(user_id);

                        ALTER TABLE suppliers
                            ADD CONSTRAINT fk_suppliers_updated_by
                            FOREIGN KEY (updated_by) REFERENCES users(user_id);
                    END IF;
                END $$;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "suppliers");
        }
    }
}
