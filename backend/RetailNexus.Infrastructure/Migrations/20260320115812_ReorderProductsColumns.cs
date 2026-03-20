using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailNexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReorderProductsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TABLE products_reordered
                (
                    "Id" uuid NOT NULL,
                    "ProductCode" character varying(50) NOT NULL,
                    "JanCode" character varying(32) NOT NULL,
                    "ProductName" character varying(200) NOT NULL,
                    "ProductCategoryCode" character varying(50) NOT NULL,
                    "Price" numeric(12,2) NOT NULL,
                    "Cost" numeric(12,2) NOT NULL,
                    "IsActive" boolean NOT NULL,
                    "UpdatedAt" timestamp with time zone NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_products_reordered" PRIMARY KEY ("Id")
                );

                INSERT INTO products_reordered
                (
                    "Id",
                    "ProductCode",
                    "JanCode",
                    "ProductName",
                    "ProductCategoryCode",
                    "Price",
                    "Cost",
                    "IsActive",
                    "UpdatedAt",
                    "CreatedAt"
                )
                SELECT
                    "Id",
                    "ProductCode",
                    "JanCode",
                    "ProductName",
                    "CategoryCode",
                    "Price",
                    "Cost",
                    "IsActive",
                    "UpdatedAt",
                    "CreatedAt"
                FROM products;

                DROP TABLE products;

                ALTER TABLE products_reordered RENAME TO products;

                CREATE UNIQUE INDEX "IX_products_ProductCode" ON products ("ProductCode");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TABLE products_original_order
                (
                    "Id" uuid NOT NULL,
                    "JanCode" character varying(32) NOT NULL,
                    "ProductCode" character varying(50) NOT NULL,
                    "ProductName" character varying(200) NOT NULL,
                    "CategoryCode" character varying(50) NOT NULL,
                    "Price" numeric(12,2) NOT NULL,
                    "Cost" numeric(12,2) NOT NULL,
                    "IsActive" boolean NOT NULL,
                    "UpdatedAt" timestamp with time zone NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_products_original_order" PRIMARY KEY ("Id")
                );

                INSERT INTO products_original_order
                (
                    "Id",
                    "JanCode",
                    "ProductCode",
                    "ProductName",
                    "CategoryCode",
                    "Price",
                    "Cost",
                    "IsActive",
                    "UpdatedAt",
                    "CreatedAt"
                )
                SELECT
                    "Id",
                    "JanCode",
                    "ProductCode",
                    "ProductName",
                    "ProductCategoryCode",
                    "Price",
                    "Cost",
                    "IsActive",
                    "UpdatedAt",
                    "CreatedAt"
                FROM products;

                DROP TABLE products;

                ALTER TABLE products_original_order RENAME TO products;

                CREATE UNIQUE INDEX "IX_products_ProductCode" ON products ("ProductCode");
                """);
        }
    }
}
