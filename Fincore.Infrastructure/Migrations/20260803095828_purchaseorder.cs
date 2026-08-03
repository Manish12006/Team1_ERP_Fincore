using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fincore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class purchaseorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Users_ApprovedBy",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Users_RequestedBy",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "OrderDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "RequiredTillDate",
                table: "PurchaseOrders");

            migrationBuilder.RenameColumn(
                name: "RequestedBy",
                table: "PurchaseOrders",
                newName: "UserId1");

            migrationBuilder.RenameColumn(
                name: "ApprovedBy",
                table: "PurchaseOrders",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrders_RequestedBy",
                table: "PurchaseOrders",
                newName: "IX_PurchaseOrders_UserId1");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrders_ApprovedBy",
                table: "PurchaseOrders",
                newName: "IX_PurchaseOrders_UserId");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "PurchaseOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "GRNItemId",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GRNItems",
                columns: table => new
                {
                    GRNItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GRNId = table.Column<int>(type: "int", nullable: false),
                    POItemId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GRNItems", x => x.GRNItemId);
                    table.ForeignKey(
                        name: "FK_GRNItems_GRNs_GRNId",
                        column: x => x.GRNId,
                        principalTable: "GRNs",
                        principalColumn: "GRNId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GRNItems_PurchaseOrderItems_POItemId",
                        column: x => x.POItemId,
                        principalTable: "PurchaseOrderItems",
                        principalColumn: "POItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_GRNItemId",
                table: "Assets",
                column: "GRNItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNItems_GRNId",
                table: "GRNItems",
                column: "GRNId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNItems_POItemId",
                table: "GRNItems",
                column: "POItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_GRNItems_GRNItemId",
                table: "Assets",
                column: "GRNItemId",
                principalTable: "GRNItems",
                principalColumn: "GRNItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Users_UserId",
                table: "PurchaseOrders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Users_UserId1",
                table: "PurchaseOrders",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_GRNItems_GRNItemId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Users_UserId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Users_UserId1",
                table: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "GRNItems");

            migrationBuilder.DropIndex(
                name: "IX_Assets_GRNItemId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "GRNItemId",
                table: "Assets");

            migrationBuilder.RenameColumn(
                name: "UserId1",
                table: "PurchaseOrders",
                newName: "RequestedBy");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "PurchaseOrders",
                newName: "ApprovedBy");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrders_UserId1",
                table: "PurchaseOrders",
                newName: "IX_PurchaseOrders_RequestedBy");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrders_UserId",
                table: "PurchaseOrders",
                newName: "IX_PurchaseOrders_ApprovedBy");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "PurchaseOrders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OrderDate",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequiredTillDate",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Users_ApprovedBy",
                table: "PurchaseOrders",
                column: "ApprovedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Users_RequestedBy",
                table: "PurchaseOrders",
                column: "RequestedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
