using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menulo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLogoUpdateTimestampToRestaurant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LogoUpdatedAtUtc",
                schema: "dbo",
                table: "Restaurants",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUpdatedAtUtc",
                schema: "dbo",
                table: "Restaurants");
        }
    }
}
