using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailNexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameCdToCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "store_cd",
                table: "stores",
                newName: "store_code");

            migrationBuilder.RenameColumn(
                name: "store_type_cd",
                table: "store_types",
                newName: "store_type_code");

            migrationBuilder.RenameColumn(
                name: "product_category_cd",
                table: "product_categories",
                newName: "product_category_code");

            migrationBuilder.RenameColumn(
                name: "area_cd",
                table: "areas",
                newName: "area_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "store_code",
                table: "stores",
                newName: "store_cd");

            migrationBuilder.RenameColumn(
                name: "store_type_code",
                table: "store_types",
                newName: "store_type_cd");

            migrationBuilder.RenameColumn(
                name: "product_category_code",
                table: "product_categories",
                newName: "product_category_cd");

            migrationBuilder.RenameColumn(
                name: "area_code",
                table: "areas",
                newName: "area_cd");
        }
    }
}
