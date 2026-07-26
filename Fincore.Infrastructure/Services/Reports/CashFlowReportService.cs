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
    public class CashFlowReportService : ICashFlowReportService
    {
        private readonly AppDbContext db;
        private readonly IMemoryCache memoryCache;

        public CashFlowReportService(
            AppDbContext db,
            IMemoryCache memoryCache)
        {
            this.db = db;
            this.memoryCache = memoryCache;
        }

        public async Task<ApiResponse<CashFlowReportDTO>> GetCashFlowAsync(int? companyId,int? departmentId, DateTime? fromDate, DateTime? toDate)
        {
            if (companyId.HasValue && companyId <= 0)
            {
                return ApiResponseHelper.Failure<CashFlowReportDTO>(
                    "Invalid company id.","INVALID_COMPANY_ID","Company id must be greater than 0.");
            }

            if (departmentId.HasValue && departmentId <= 0)
            {
                return ApiResponseHelper.Failure<CashFlowReportDTO>(
                    "Invalid department id.","INVALID_DEPARTMENT_ID", "Department id must be greater than 0.");
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return ApiResponseHelper.Failure<CashFlowReportDTO>(
                    "Invalid date range.","INVALID_DATE_RANGE","From date cannot be greater than To date.");
            }

            string cacheKey = $"CashFlow_{companyId}_{departmentId}_{fromDate}_{toDate}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<CashFlowReportDTO> cachedResponse))
            {
                return cachedResponse;
            }

            // Revenue Query (Cash Inflow)
            var revenueQuery = db.RevenueEntries
                .Include(r => r.Department)
                    .ThenInclude(d => d.Company)
                .AsQueryable();

            // Expense Query (Cash Outflow)
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
                revenueQuery = revenueQuery.Where(r =>r.Department.CompanyId == companyId.Value);

                expenseQuery = expenseQuery.Where(e =>e.OpexRequest.BudgetLine.BudgetCategory.Department.CompanyId == companyId.Value);
            }

            // Department Filter
            if (departmentId.HasValue)
            {
                revenueQuery = revenueQuery.Where(r =>r.DepartmentId == departmentId.Value);

                expenseQuery = expenseQuery.Where(e => e.OpexRequest.BudgetLine.BudgetCategory.Department.DepartmentId == departmentId.Value);
            }

            // From Date Filter
            if (fromDate.HasValue)
            {
                revenueQuery = revenueQuery.Where(r => r.RevenueDate >= fromDate.Value);

                expenseQuery = expenseQuery.Where(e => e.ExpenseDate >= fromDate.Value);
            }

            // To Date Filter
            if (toDate.HasValue)
            {
                revenueQuery = revenueQuery.Where(r =>r.RevenueDate <= toDate.Value);

                expenseQuery = expenseQuery.Where(e => e.ExpenseDate <= toDate.Value);
            }

            decimal cashInflow = await revenueQuery.SumAsync(r => (decimal?)r.Amount) ?? 0;

            decimal cashOutflow = await expenseQuery.SumAsync(e => (decimal?)e.ExpenseAmount) ?? 0;

            decimal netCashFlow = cashInflow - cashOutflow;

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

            string cashFlowStatus;

            if (netCashFlow > 0)
            {
                cashFlowStatus = "Positive Cash Flow";
            }
            else if (netCashFlow < 0)
            {
                cashFlowStatus = "Negative Cash Flow";
            }
            else
            {
                cashFlowStatus = "Break Even";
            }

            var result = new CashFlowReportDTO
            {
                CompanyName = companyName,
                DepartmentName = departmentName,
                CashInflow = cashInflow,
                CashOutflow = cashOutflow,
                NetCashFlow = netCashFlow,
                CashFlowStatus = cashFlowStatus
            };

            var response = ApiResponseHelper.SuccessRes(result,"Cash Flow report fetched successfully.");

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }
    }
}
