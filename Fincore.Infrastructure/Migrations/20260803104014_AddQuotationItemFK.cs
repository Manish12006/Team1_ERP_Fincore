using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fincore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationItemFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "PurchaseOrderItems");

            migrationBuilder.AddColumn<int>(
                name: "QuotationItemId",
                table: "PurchaseOrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_QuotationItemId",
                table: "PurchaseOrderItems",
                column: "QuotationItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_QuotationItems_QuotationItemId",
                table: "PurchaseOrderItems",
                column: "QuotationItemId",
                principalTable: "QuotationItems",
                principalColumn: "QuotationItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_QuotationItems_QuotationItemId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderItems_QuotationItemId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "QuotationItemId",
                table: "PurchaseOrderItems");

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "PurchaseOrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
