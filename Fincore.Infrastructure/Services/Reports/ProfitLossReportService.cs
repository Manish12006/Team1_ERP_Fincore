using Fincore.Application.DTO;
using Fincore.Application.DTO.Reports;
using Fincore.Application.Interfaces.Reports;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Infrastructure.Services.Reports
{
    public class ProfitLossReportService : IProfitLossReportService
    {
        private readonly AppDbContext db;
        private readonly IMemoryCache memoryCache;

        public ProfitLossReportService(
            AppDbContext db,
            IMemoryCache memoryCache)
        {
            this.db = db;
            this.memoryCache = memoryCache;
        }

        public async Task<ApiResponse<ProfitLossReportDTO>> GetProfitLossAsync( int? companyId,int? departmentId, DateTime? fromDate, DateTime? toDate)
        {
            if (companyId.HasValue && companyId <= 0)
            {
                return ApiResponseHelper.Failure<ProfitLossReportDTO>(
                    "Invalid company ID.","INVALID_COMPANY_ID","Company ID must be greater than 0.");
            }

            if (departmentId.HasValue && departmentId <= 0)
            {
                return ApiResponseHelper.Failure<ProfitLossReportDTO>(
                    "Invalid department ID.","INVALID_DEPARTMENT_ID","Department ID must be greater than 0.");
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return ApiResponseHelper.Failure<ProfitLossReportDTO>(
                    "Invalid date range.","INVALID_DATE_RANGE","From date cannot be greater than To date.");
            }

            string cacheKey = $"ProfitLoss_{companyId}_{departmentId}_{fromDate}_{toDate}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<ProfitLossReportDTO> cachedResponse))
            {
                return cachedResponse;
            }

            // Revenue Query
            var revenueQuery = db.RevenueEntries
                .Include(r => r.Department)
                    .ThenInclude(d => d.Company)
                .AsQueryable();

            // Expense Query
            var expenseQuery = db.ExpenseClaims
                .Include(e => e.OpexRequest)
                    .ThenInclude(o => o.BudgetLine)
                        .ThenInclude(b => b.BudgetCategory)
                            .ThenInclude(c => c.Department)
                                .ThenInclude(d => d.Company)
                .AsQueryable();

            // Company Filter
            if (companyId.HasValue)
            {
                revenueQuery = revenueQuery.Where(r =>
                    r.Department.CompanyId == companyId.Value);

                expenseQuery = expenseQuery.Where(e =>
                    e.OpexRequest.BudgetLine.BudgetCategory.Department.CompanyId == companyId.Value);
            }

            // Department Filter
            if (departmentId.HasValue)
            {
                revenueQuery = revenueQuery.Where(r =>
                    r.DepartmentId == departmentId.Value);

                expenseQuery = expenseQuery.Where(e =>
                    e.OpexRequest.BudgetLine.BudgetCategory.Department.DepartmentId == departmentId.Value);
            }

            // From Date Filter
            if (fromDate.HasValue)
            {
                revenueQuery = revenueQuery.Where(r =>
                    r.RevenueDate >= fromDate.Value);

                expenseQuery = expenseQuery.Where(e =>
                    e.ExpenseDate >= fromDate.Value);
            }

            // To Date Filter
            if (toDate.HasValue)
            {
                revenueQuery = revenueQuery.Where(r =>
                    r.RevenueDate <= toDate.Value);

                expenseQuery = expenseQuery.Where(e =>
                    e.ExpenseDate <= toDate.Value);
            }

            decimal totalRevenue = await revenueQuery.SumAsync(r => (decimal?)r.Amount) ?? 0;

            decimal totalExpense = await expenseQuery.SumAsync(e => (decimal?)e.ExpenseAmount) ?? 0;

            decimal profit = 0;
            decimal loss = 0;

            if (totalRevenue > totalExpense)
            {
                profit = totalRevenue - totalExpense;
            }
            else if (totalExpense > totalRevenue)
            {
                loss = totalExpense - totalRevenue;
            }

            string companyName = "All Companies";

            if (companyId.HasValue)
            {
                companyName = await db.Companies
                    .Where(c => c.CompanyId == companyId.Value)
                    .Select(c => c.CompanyName)
                    .FirstOrDefaultAsync() ?? "N/A";
            }

            string departmentName = "All Departments";

            if (departmentId.HasValue)
            {
                departmentName = await db.Departments
                    .Where(d => d.DepartmentId == departmentId.Value)
                    .Select(d => d.DepartmentName)
                    .FirstOrDefaultAsync() ?? "N/A";
            }

            var result = new ProfitLossReportDTO
            {
                CompanyName = companyName,
                DepartmentName = departmentName,
                TotalRevenue = totalRevenue,
                TotalExpense = totalExpense,
                Profit = profit,
                Loss = loss
            };

            var response = ApiResponseHelper.SuccessRes(
                result,
                "Profit & Loss report fetched successfully.");

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }
    }
}
