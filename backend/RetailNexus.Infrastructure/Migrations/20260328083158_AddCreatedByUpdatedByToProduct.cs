using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailNexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByUpdatedByToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                table: "products",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by",
                table: "products");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "products");
        }
    }
}
