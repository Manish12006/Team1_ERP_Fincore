using Fincore.Application.DTO;
using Fincore.Application.DTO.Dashboard;
using Fincore.Application.Interfaces.Dashboard;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
namespace Fincore.Infrastructure.Services.Dashboard
{
    public class FinanceService : IFinanceService
    {
         AppDbContext db;
        IMemoryCache cache;

        public FinanceService(AppDbContext db,IMemoryCache cache)
        {
            this.db = db;
            this.cache = cache;
        }

        public async Task<ApiResponse<FinanceDashboardDto>> GetFinanceDashboard()
        {
            const string cacheKey = "FinanceDashboard";
            if (!cache.TryGetValue(cacheKey, out FinanceDashboardDto dashboard))
            {
                 dashboard = new FinanceDashboardDto
                {
                    // Revenue Total
                    TotalRevenue = await db.RevenueEntries
                    .SumAsync(x => (decimal?)x.Amount) ?? 0,

                    // Payments Total &  Approval Pending
                    TotalPayments = await db.Payments
                    .SumAsync(x => (decimal?)x.Amount) ?? 0,

                    ApprovedPayments = await db.Payments
                    .Where(x => x.ApprovalStatus == "Approved")
                    .SumAsync(x => (decimal?)x.Amount) ?? 0,

                    PendingPayments = await db.Payments
                    .Where(x => x.ApprovalStatus == "Pending")
                    .SumAsync(x => (decimal?)x.Amount) ?? 0,

                    // Purchase Orders Count, Approval ,Pending
                    TotalPurchaseOrders = await db.PurchaseOrders
                    .CountAsync(),

                    ApprovedPurchaseOrders = await db.PurchaseOrders
                    .Where(x => x.ApprovalStatus == "Approved")
                    .CountAsync(),

                    PendingPurchaseOrders = await db.PurchaseOrders
                    .Where(x => x.ApprovalStatus == "Pending")
                    .CountAsync(),

                    PurchaseOrderAmount = await db.PurchaseOrders
                    .SumAsync(x => (decimal?)x.Amount) ?? 0,

                    // Purchase Requisitions count Approval,pending
                    TotalPurchaseRequisitions = await db.PurchaseRequisitions
                    .CountAsync(),

                    ApprovedPurchaseRequisitions = await db.PurchaseRequisitions
                    .Where(x => x.ApprovalStatus == "Approved")
                    .CountAsync(),

                    PendingPurchaseRequisitions = await db.PurchaseRequisitions
                    .Where(x => x.ApprovalStatus == "Pending")
                    .CountAsync(),

                };
                cache.Set(cacheKey, dashboard, TimeSpan.FromMinutes(5));
            }
                return ApiResponseHelper.SuccessRes(
                dashboard,
                "Finance Dashboard fetched successfully");
        }
    }
}