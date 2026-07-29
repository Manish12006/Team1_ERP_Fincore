using Fincore.Application.DTO;
using Fincore.Application.DTO.Dashboard;
using Fincore.Application.Interfaces.Dashboard;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fincore.Infrastructure.Services.Dashboard
{
    public class ProcurementService : IProcurementService
    {
         AppDbContext db;

        public ProcurementService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<ApiResponse<ProcurementDashboardDto>> GetProcurementDashboard()
        {
            var dashboard = new ProcurementDashboardDto
            {
                TotalPurchaseRequisitions = await db.PurchaseRequisitions
                .CountAsync(),
                ApprovedPurchaseRequisitions = await db.PurchaseRequisitions
                .Where(x => x.ApprovalStatus == "Approved")
                .CountAsync(),
                PendingPurchaseRequisitions = await db.PurchaseRequisitions
                .Where(x => x.ApprovalStatus == "Pending")
                .CountAsync(),

                TotalPurchaseOrders = await db.PurchaseOrders
                .CountAsync(),
                ApprovedPurchaseOrders = await db.PurchaseOrders
                .Where(x => x.ApprovalStatus == "Approved")
                .CountAsync(),
                PendingPurchaseOrders = await db.PurchaseOrders
                .Where(x => x.ApprovalStatus == "Pending")
                .CountAsync(),

                TotalPurchaseOrderAmount = await db.PurchaseOrders
                .SumAsync(x => x.Amount),
            };
            return ApiResponseHelper.SuccessRes(
            dashboard,
            "Procurement Dashboard fetched successfully"); ;
        }
    }
}