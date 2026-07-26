using AutoMapper;
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
    public class BudgetVarianceReportService : IBudgetVarianceReportService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache memoryCache;

        public BudgetVarianceReportService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache memoryCache)
        {
            this.db = db;
            this.mapper = mapper;
            this.memoryCache = memoryCache;
        }

        public async Task<ApiResponse<List<BudgetVarianceReportDTO>>> GetBudgetVarianceAsync(int page, int pageSize)
        {
            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<BudgetVarianceReportDTO>>(
                    "Invalid page number.","INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<BudgetVarianceReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE","Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"BudgetVariance_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<List<BudgetVarianceReportDTO>> cachedResponse))
            {
                return cachedResponse;
            }

            var budgetLines = await db.BudgetLines
                .Include(x => x.Budget)
                .Include(x => x.BudgetCategory).ThenInclude(x => x.Department)
                .OrderBy(x => x.BudgetLineId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!budgetLines.Any())
            {
                return ApiResponseHelper.Failure<List<BudgetVarianceReportDTO>>(
                    "No budget variance records found.","BUDGET_VARIANCE_NOT_FOUND","No budget variance records available.");
            }

            var result = mapper.Map<List<BudgetVarianceReportDTO>>(budgetLines);

            var response = ApiResponseHelper.SuccessRes(result,"Budget Variance report fetched successfully.",result.Count);

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }

        public async Task<ApiResponse<List<BudgetVarianceReportDTO>>> GetBudgetVarianceByDepartmentAsync(int departmentId, int page, int pageSize)
        {
            if (departmentId <= 0)
            {
                return ApiResponseHelper.Failure<List<BudgetVarianceReportDTO>>(
                    "Invalid department id.","INVALID_DEPARTMENT_ID","Department id must be greater than 0.");
            }

            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<BudgetVarianceReportDTO>>(
                    "Invalid page number.","INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<BudgetVarianceReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE","Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"BudgetVariance_Department_{departmentId}_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<List<BudgetVarianceReportDTO>> cachedResponse))
            {
                return cachedResponse;
            }

            var budgetLines = await db.BudgetLines
                .Include(x => x.Budget)
                .Include(x => x.BudgetCategory).ThenInclude(x => x.Department)
                .Where(x => x.BudgetCategory.DepartmentId == departmentId)
                .OrderBy(x => x.BudgetLineId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!budgetLines.Any())
            {
                return ApiResponseHelper.Failure<List<BudgetVarianceReportDTO>>(
                    "No budget variance records found.","BUDGET_VARIANCE_NOT_FOUND","No budget variance records found for the selected department.");
            }

            var result = mapper.Map<List<BudgetVarianceReportDTO>>(budgetLines);

            var response = ApiResponseHelper.SuccessRes(result,"Budget Variance report fetched successfully.",result.Count);

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }

        public async Task<ApiResponse<List<BudgetVarianceReportDTO>>> GetBudgetVarianceByFinancialYearAsync(string financialYear, int page, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(financialYear))
            {
                return ApiResponseHelper.Failure<List<BudgetVarianceReportDTO>>(
                    "Financial year is required.","INVALID_FINANCIAL_YEAR", "Please provide a valid financial year.");
            }

            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<BudgetVarianceReportDTO>>(
                    "Invalid page number.","INVALID_PAGE", "Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<BudgetVarianceReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE", "Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"BudgetVariance_FY_{financialYear}_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<List<BudgetVarianceReportDTO>> cachedResponse))
            {
                return cachedResponse;
            }

            var budgetLines = await db.BudgetLines
                .Include(x => x.Budget)
                .Include(x => x.BudgetCategory).ThenInclude(x => x.Department)
                .Where(x => x.Budget.FinancialYear == financialYear)
                .OrderBy(x => x.BudgetLineId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!budgetLines.Any())
            {
                return ApiResponseHelper.Failure<List<BudgetVarianceReportDTO>>(
                    "No budget variance records found.","BUDGET_VARIANCE_NOT_FOUND","No budget variance records found for the selected financial year.");
            }

            var result = mapper.Map<List<BudgetVarianceReportDTO>>(budgetLines);

            var response = ApiResponseHelper.SuccessRes(result,"Budget Variance report fetched successfully.",result.Count);

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }
    }
}
