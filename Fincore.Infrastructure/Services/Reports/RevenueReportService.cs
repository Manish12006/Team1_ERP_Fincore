using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Reports;
using Fincore.Application.Interfaces.Reports;
using Fincore.Domain.Enums;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Infrastructure.Services.Reports
{
    public class RevenueReportService : IRevenueReportService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache memoryCache;

        public RevenueReportService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache memoryCache)
        {
            this.db = db;
            this.mapper = mapper;
            this.memoryCache = memoryCache;
        }

        public async Task<ApiResponse<List<RevenueReportDTO>>> GetRevenueAsync(int page, int pageSize)
        {
            // Validate User Input
            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page number.","INVALID_PAGE","Page number must be greater than 0.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE","Page size must be greater than 0.");
            }

            if (pageSize > 100)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page size.", "INVALID_PAGE_SIZE","Page size cannot be greater than 100.");
            }

            // Cache Key
            string cacheKey = $"RevenueReport_{page}_{pageSize}";

            // Check Cache
            if (memoryCache.TryGetValue(cacheKey, out List<RevenueReportDTO>? cachedRevenue))
            {
                return ApiResponseHelper.SuccessRes(cachedRevenue,"Revenue report fetched successfully.",cachedRevenue.Count);
            }

            // Fetch Data
            var revenue = await db.RevenueEntries
                .Include(x => x.Customer)
                .Include(x => x.Department)
                .Include(x => x.AccountMaster)
                .OrderByDescending(x => x.RevenueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Check Data
            if (!revenue.Any())
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "No revenue records found.","REVENUE_NOT_FOUND","No revenue records are available.");
            }

            // Map DTO
            var result = mapper.Map<List<RevenueReportDTO>>(revenue);

            // Store Cache
            memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            // Success Response
            return ApiResponseHelper.SuccessRes( result,"Revenue report fetched successfully.",result.Count);
        }

        public async Task<ApiResponse<List<RevenueReportDTO>>> GetRevenueByAccountAsync(int accountId, int page, int pageSize)
        {
            // Validate User Input
            if (accountId <= 0)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid account id.","INVALID_ACCOUNT_ID","Account id must be greater than 0.");
            }

            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page number.", "INVALID_PAGE", "Page number must be greater than 0.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE","Page size must be greater than 0.");
            }

            if (pageSize > 100)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE","Page size cannot be greater than 100.");
            }

            string cacheKey = $"RevenueAccount_{accountId}_{page}_{pageSize}";

            // Check Cache
            if (memoryCache.TryGetValue(cacheKey, out List<RevenueReportDTO>? cachedRevenue))
            {
                return ApiResponseHelper.SuccessRes(cachedRevenue, "Account revenue report fetched successfully.",cachedRevenue.Count);
            }

            // Fetch Data
            var revenue = await db.RevenueEntries
                .Include(x => x.Customer)
                .Include(x => x.Department)
                .Include(x => x.AccountMaster)
                .Where(x => x.AccountId == accountId)
                .OrderByDescending(x => x.RevenueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!revenue.Any())
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "No revenue records found.", "REVENUE_NOT_FOUND","No revenue records found for the specified account.");
            }

            // AutoMapper
            var result = mapper.Map<List<RevenueReportDTO>>(revenue);

            // Store Cache
            memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(
                result, "Account revenue report fetched successfully.",result.Count);
        }

        public async Task<ApiResponse<List<RevenueReportDTO>>> GetRevenueByCustomerAsync(int customerId, int page, int pageSize)
        {
            // Validate User Input
            if (customerId <= 0)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid customer id.","INVALID_CUSTOMER_ID","Customer id must be greater than 0.");
            }

            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>("Invalid page number.","INVALID_PAGE","Page number must be greater than 0.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>( "Invalid page size.", "INVALID_PAGE_SIZE","Page size must be greater than 0.");
            }

            if (pageSize > 100)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>( "Invalid page size.","INVALID_PAGE_SIZE","Page size cannot be greater than 100.");
            }

            string cacheKey = $"RevenueCustomer_{customerId}_{page}_{pageSize}";

            // Check Cache
            if (memoryCache.TryGetValue(cacheKey, out List<RevenueReportDTO>? cachedRevenue))
            {
                return ApiResponseHelper.SuccessRes( cachedRevenue,"Customer revenue report fetched successfully.",cachedRevenue.Count);
            }

            // Fetch Data
            var revenue = await db.RevenueEntries
                .Include(x => x.Customer)
                .Include(x => x.Department)
                .Include(x => x.AccountMaster)
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.RevenueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!revenue.Any())
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "No revenue records found.","REVENUE_NOT_FOUND","No revenue records found for the specified customer.");
            }

            // AutoMapper
            var result = mapper.Map<List<RevenueReportDTO>>(revenue);

            // Store Cache
            memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(result, "Customer revenue report fetched successfully.", result.Count);
        }


        public async Task<ApiResponse<List<RevenueReportDTO>>> GetRevenueByDateRangeAsync(DateTime fromDate, DateTime toDate, int page,int pageSize)
        {
            // Validate User Input
            if (fromDate == DateTime.MinValue || toDate == DateTime.MinValue)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid date.","INVALID_DATE", "Please provide valid From Date and To Date.");
            }

            if (fromDate > toDate)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid date range.","INVALID_DATE_RANGE","From Date cannot be greater than To Date.");
            }

            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page number.", "INVALID_PAGE","Page number must be greater than 0.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page size.", "INVALID_PAGE_SIZE","Page size must be greater than 0.");
            }

            if (pageSize > 100)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page size.", "INVALID_PAGE_SIZE","Page size cannot be greater than 100.");
            }

            string cacheKey = $"RevenueDateRange_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}_{page}_{pageSize}";

            // Check Cache
            if (memoryCache.TryGetValue(cacheKey, out List<RevenueReportDTO>? cachedRevenue))
            {
                return ApiResponseHelper.SuccessRes(
                    cachedRevenue, "Revenue report fetched successfully.", cachedRevenue.Count);
            }

            // Fetch Data
            var revenue = await db.RevenueEntries
                .Include(x => x.Customer)
                .Include(x => x.Department)
                .Include(x => x.AccountMaster)
                .Where(x => x.RevenueDate >= fromDate && x.RevenueDate <= toDate)
                .OrderByDescending(x => x.RevenueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!revenue.Any())
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "No revenue records found.","REVENUE_NOT_FOUND","No revenue records found for the specified date range.");
            }

            // AutoMapper
            var result = mapper.Map<List<RevenueReportDTO>>(revenue);

            // Store Cache
            memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            // Success Response
            return ApiResponseHelper.SuccessRes( result,"Revenue report fetched successfully.", result.Count);
        }

        public async Task<ApiResponse<List<RevenueReportDTO>>> GetRevenueByDepartmentAsync(int departmentId, int page, int pageSize)
        {
            // Validate User Input
            if (departmentId <= 0)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid department id.","INVALID_DEPARTMENT_ID","Department id must be greater than 0.");
            }

            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page number.","INVALID_PAGE","Page number must be greater than 0.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE", "Page size must be greater than 0.");
            }

            if (pageSize > 100)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page size.", "INVALID_PAGE_SIZE","Page size cannot be greater than 100.");
            }

            string cacheKey = $"RevenueDepartment_{departmentId}_{page}_{pageSize}";

            // Check Cache
            if (memoryCache.TryGetValue(cacheKey, out List<RevenueReportDTO>? cachedRevenue))
            {
                return ApiResponseHelper.SuccessRes(
                    cachedRevenue, "Department revenue report fetched successfully.", cachedRevenue.Count);
            }

            // Fetch Data
            var revenue = await db.RevenueEntries
                .Include(x => x.Customer)
                .Include(x => x.Department)
                .Include(x => x.AccountMaster)
                .Where(x => x.DepartmentId == departmentId)
                .OrderByDescending(x => x.RevenueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!revenue.Any())
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "No revenue records found.","REVENUE_NOT_FOUND", "No revenue records found for the specified department.");
            }

            // AutoMapper
            var result = mapper.Map<List<RevenueReportDTO>>(revenue);

            // Store Cache
            memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(
                result, "Department revenue report fetched successfully.", result.Count);
        }

        public async Task<ApiResponse<List<RevenueReportDTO>>> GetRevenueByRevenueTypeAsync(RevenueType revenueType, int page, int pageSize)
        {
            // Validate User Input
            if (!Enum.IsDefined(typeof(RevenueType), revenueType))
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid revenue type.", "INVALID_REVENUE_TYPE","Please provide a valid revenue type.");
            }

            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page number.", "INVALID_PAGE", "Page number must be greater than 0.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE","Page size must be greater than 0.");
            }

            if (pageSize > 100)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE", "Page size cannot be greater than 100.");
            }

            string cacheKey = $"RevenueType_{revenueType}_{page}_{pageSize}";

            // Check Cache
            if (memoryCache.TryGetValue(cacheKey, out List<RevenueReportDTO>? cachedRevenue))
            {
                return ApiResponseHelper.SuccessRes( cachedRevenue, "Revenue type report fetched successfully.",cachedRevenue.Count);
            }

            // Fetch Data
            var revenue = await db.RevenueEntries
                .Include(x => x.Customer)
                .Include(x => x.Department)
                .Include(x => x.AccountMaster)
                .Where(x => x.RevenueType == revenueType.ToString())
                .OrderByDescending(x => x.RevenueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!revenue.Any())
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "No revenue records found.", "REVENUE_NOT_FOUND","No revenue records found for the specified revenue type.");
            }

            // AutoMapper
            var result = mapper.Map<List<RevenueReportDTO>>(revenue);

            // Store Cache
            memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(
                result,"Revenue type report fetched successfully.", result.Count);
        }

        public async Task<ApiResponse<List<RevenueReportDTO>>> GetRevenueByStatusAsync(RevenueStatus status, int page, int pageSize)
        {
            // Validate User Input
            if (!Enum.IsDefined(typeof(RevenueStatus), status))
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid revenue status.", "INVALID_REVENUE_STATUS","Please provide a valid revenue status.");
            }

            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page number.", "INVALID_PAGE","Page number must be greater than 0.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page size.", "INVALID_PAGE_SIZE", "Page size must be greater than 0.");
            }

            if (pageSize > 100)
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE", "Page size cannot be greater than 100.");
            }

            string cacheKey = $"RevenueStatus_{status}_{page}_{pageSize}";

            // Check Cache
            if (memoryCache.TryGetValue(cacheKey, out List<RevenueReportDTO>? cachedRevenue))
            {
                return ApiResponseHelper.SuccessRes( cachedRevenue, "Revenue status report fetched successfully.",cachedRevenue.Count);
            }

            // Fetch Data
            var revenue = await db.RevenueEntries
                .Include(x => x.Customer)
                .Include(x => x.Department)
                .Include(x => x.AccountMaster)
                .Where(x => x.Status == status.ToString())
                .OrderByDescending(x => x.RevenueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!revenue.Any())
            {
                return ApiResponseHelper.Failure<List<RevenueReportDTO>>(
                    "No revenue records found.","REVENUE_NOT_FOUND","No revenue records found for the specified revenue status.");
            }

            // AutoMapper
            var result = mapper.Map<List<RevenueReportDTO>>(revenue);

            // Store Cache
            memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(result,"Revenue status report fetched successfully.",result.Count);
        }
    }
}
