using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fincore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuotationItemFK1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_QuotationItems_QuotationItemId",
                table: "PurchaseOrderItems");

            migrationBuilder.AlterColumn<int>(
                name: "QuotationItemId",
                table: "PurchaseOrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_QuotationItems_QuotationItemId",
                table: "PurchaseOrderItems",
                column: "QuotationItemId",
                principalTable: "QuotationItems",
                principalColumn: "QuotationItemId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_QuotationItems_QuotationItemId",
                table: "PurchaseOrderItems");

            migrationBuilder.AlterColumn<int>(
                name: "QuotationItemId",
                table: "PurchaseOrderItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_QuotationItems_QuotationItemId",
                table: "PurchaseOrderItems",
                column: "QuotationItemId",
                principalTable: "QuotationItems",
                principalColumn: "QuotationItemId");
        }
    }
}
