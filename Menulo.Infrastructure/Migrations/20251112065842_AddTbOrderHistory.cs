using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menulo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTbOrderHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderId",
                schema: "dbo",
                table: "OrderItems");

            migrationBuilder.AddColumn<int>(
                name: "OrderHistoryId",
                schema: "dbo",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OrderHistorys",
                schema: "dbo",
                columns: table => new
                {
                    OrderHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    RoundNo = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderHistorys", x => x.OrderHistoryId);
                    table.ForeignKey(
                        name: "FK_OrderHistorys_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "dbo",
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderHistoryId",
                schema: "dbo",
                table: "OrderItems",
                column: "OrderHistoryId");

            migrationBuilder.CreateIndex(
                name: "UQ_OrderItem_Order_Item_History",
                schema: "dbo",
                table: "OrderItems",
                columns: new[] { "OrderId", "ItemId", "OrderHistoryId" },
                unique: true,
                filter: "[ItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_OrderHistory_Order_Round",
                schema: "dbo",
                table: "OrderHistorys",
                columns: new[] { "OrderId", "RoundNo" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_OrderHistorys_OrderHistoryId",
                schema: "dbo",
                table: "OrderItems",
                column: "OrderHistoryId",
                principalSchema: "dbo",
                principalTable: "OrderHistorys",
                principalColumn: "OrderHistoryId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_OrderHistorys_OrderHistoryId",
                schema: "dbo",
                table: "OrderItems");

            migrationBuilder.DropTable(
                name: "OrderHistorys",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderHistoryId",
                schema: "dbo",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "UQ_OrderItem_Order_Item_History",
                schema: "dbo",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "OrderHistoryId",
                schema: "dbo",
                table: "OrderItems");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                schema: "dbo",
                table: "OrderItems",
                column: "OrderId");
        }
    }
}
