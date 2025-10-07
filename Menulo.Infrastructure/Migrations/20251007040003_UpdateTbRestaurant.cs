using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menulo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTbRestaurant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoImage",
                schema: "dbo",
                table: "Restaurants");

            migrationBuilder.AlterColumn<string>(
                name: "StaticQrImageUrl",
                schema: "dbo",
                table: "Restaurants",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                schema: "dbo",
                table: "Restaurants",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                schema: "dbo",
                table: "Restaurants");

            migrationBuilder.AlterColumn<byte[]>(
                name: "StaticQrImageUrl",
                schema: "dbo",
                table: "Restaurants",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "LogoImage",
                schema: "dbo",
                table: "Restaurants",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
