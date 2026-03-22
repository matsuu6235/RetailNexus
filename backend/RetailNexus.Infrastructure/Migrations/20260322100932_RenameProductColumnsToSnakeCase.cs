using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailNexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameProductColumnsToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "products",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "Cost",
                table: "products",
                newName: "cost");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "products",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "products",
                newName: "product_name");

            migrationBuilder.RenameColumn(
                name: "ProductCode",
                table: "products",
                newName: "product_code");

            migrationBuilder.RenameColumn(
                name: "ProductCategoryCode",
                table: "products",
                newName: "product_category_code");

            migrationBuilder.RenameColumn(
                name: "JanCode",
                table: "products",
                newName: "jan_code");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "products",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "products",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "products",
                newName: "product_id");

            migrationBuilder.RenameIndex(
                name: "IX_products_ProductCode",
                table: "products",
                newName: "IX_products_product_code");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "updated_at",
                table: "products",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "products",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "product_id",
                table: "products",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "price",
                table: "products",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "cost",
                table: "products",
                newName: "Cost");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "products",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "product_name",
                table: "products",
                newName: "ProductName");

            migrationBuilder.RenameColumn(
                name: "product_code",
                table: "products",
                newName: "ProductCode");

            migrationBuilder.RenameColumn(
                name: "product_category_code",
                table: "products",
                newName: "ProductCategoryCode");

            migrationBuilder.RenameColumn(
                name: "jan_code",
                table: "products",
                newName: "JanCode");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "products",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "products",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "products",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_products_product_code",
                table: "products",
                newName: "IX_products_ProductCode");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "products",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "products",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "products",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "products",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");
        }
    }
}
