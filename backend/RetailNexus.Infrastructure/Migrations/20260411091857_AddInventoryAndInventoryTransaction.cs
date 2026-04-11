using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailNexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryAndInventoryTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inventories",
                columns: table => new
                {
                    inventory_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventories", x => x.inventory_id);
                    table.ForeignKey(
                        name: "FK_inventories_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventories_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "store_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inventory_transactions",
                columns: table => new
                {
                    inventory_transaction_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_type = table.Column<int>(type: "integer", nullable: false),
                    quantity_change = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    quantity_after = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reference_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_transactions", x => x.inventory_transaction_id);
                    table.ForeignKey(
                        name: "FK_inventory_transactions_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventory_transactions_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "store_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inventories_product_id_store_id",
                table: "inventories",
                columns: new[] { "product_id", "store_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inventory_transactions_store_id_product_id_occurred_at",
                table: "inventory_transactions",
                columns: new[] { "store_id", "product_id", "occurred_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventories");

            migrationBuilder.DropTable(
                name: "inventory_transactions");
        }
    }
}
