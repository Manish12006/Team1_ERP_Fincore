using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fincore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseRequisitionIdToQuotation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "IsActive",
                table: "Quotations",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseRequisitionId",
                table: "Quotations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_PurchaseRequisitionId",
                table: "Quotations",
                column: "PurchaseRequisitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_PurchaseRequisitions_PurchaseRequisitionId",
                table: "Quotations",
                column: "PurchaseRequisitionId",
                principalTable: "PurchaseRequisitions",
                principalColumn: "PurchaseRequisitionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_PurchaseRequisitions_PurchaseRequisitionId",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_PurchaseRequisitionId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "PurchaseRequisitionId",
                table: "Quotations");
        }
    }
}
