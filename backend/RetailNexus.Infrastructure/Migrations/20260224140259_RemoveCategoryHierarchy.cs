using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailNexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoryHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categories_categories_ParentCategoryId",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "IX_categories_ParentCategoryId",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "IX_categories_SortOrder",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "ParentCategoryId",
                table: "categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentCategoryId",
                table: "categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_ParentCategoryId",
                table: "categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_categories_SortOrder",
                table: "categories",
                column: "SortOrder");

            migrationBuilder.AddForeignKey(
                name: "FK_categories_categories_ParentCategoryId",
                table: "categories",
                column: "ParentCategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
