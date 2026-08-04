using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fincore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseRequisitionIdToQuotation1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_PurchaseRequisitions_PurchaseRequisitionId",
                table: "Quotations");

            migrationBuilder.AlterColumn<int>(
                name: "PurchaseRequisitionId",
                table: "Quotations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_PurchaseRequisitions_PurchaseRequisitionId",
                table: "Quotations",
                column: "PurchaseRequisitionId",
                principalTable: "PurchaseRequisitions",
                principalColumn: "PurchaseRequisitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_PurchaseRequisitions_PurchaseRequisitionId",
                table: "Quotations");

            migrationBuilder.AlterColumn<int>(
                name: "PurchaseRequisitionId",
                table: "Quotations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_PurchaseRequisitions_PurchaseRequisitionId",
                table: "Quotations",
                column: "PurchaseRequisitionId",
                principalTable: "PurchaseRequisitions",
                principalColumn: "PurchaseRequisitionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
