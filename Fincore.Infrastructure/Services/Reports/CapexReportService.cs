using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Reports;
using Fincore.Application.Interfaces.Reports;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Fincore.Domain.Enums;
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
    public class CapexReportService : ICapexReportService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache memoryCache;

        public CapexReportService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache memoryCache)
        {
            this.db = db;
            this.mapper = mapper;
            this.memoryCache = memoryCache;
        }

        public async Task<ApiResponse<List<CapexReportDTO>>> GetCapexAsync(int page, int pageSize)
        {
            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<CapexReportDTO>>(
                    "Invalid page number.","INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<CapexReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE","Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"Capex_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<List<CapexReportDTO>> cachedResponse))
            {
                return cachedResponse;
            }

            var capexRequests = await db.CapexRequests
                .Include(x => x.Department)
                .Include(x => x.RequestedByUser)
                .Include(x => x.BudgetLine)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!capexRequests.Any())
            {
                return ApiResponseHelper.Failure<List<CapexReportDTO>>(
                    "No Capex records found.", "CAPEX_NOT_FOUND","No Capex requests available.");
            }

            var result = mapper.Map<List<CapexReportDTO>>(capexRequests);

            var response = ApiResponseHelper.SuccessRes(result,"Capex report fetched successfully.",result.Count);

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }

        public async Task<ApiResponse<List<CapexReportDTO>>> GetCapexByDepartmentAsync(int departmentId, int page, int pageSize)
        {
            if (departmentId <= 0)
            {
                return ApiResponseHelper.Failure<List<CapexReportDTO>>(
                    "Invalid department id.","INVALID_DEPARTMENT_ID", "Department id must be greater than 0.");
            }

            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<CapexReportDTO>>(
                    "Invalid page number.","INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<CapexReportDTO>>(
                    "Invalid page size.", "INVALID_PAGE_SIZE","Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"Capex_Department_{departmentId}_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<List<CapexReportDTO>> cachedResponse))
            {
                return cachedResponse;
            }

            var capexRequests = await db.CapexRequests
                .Include(x => x.Department)
                .Include(x => x.RequestedByUser)
                .Include(x => x.BudgetLine)
                .Where(x => x.DepartmentId == departmentId)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!capexRequests.Any())
            {
                return ApiResponseHelper.Failure<List<CapexReportDTO>>(
                    "No Capex records found.","CAPEX_NOT_FOUND","No Capex requests found for the selected department.");
            }

            var result = mapper.Map<List<CapexReportDTO>>(capexRequests);

            var response = ApiResponseHelper.SuccessRes(result,"Capex report fetched successfully.",result.Count);

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }

        public async Task<ApiResponse<List<CapexReportDTO>>> GetCapexByStatusAsync(ApprovalStatus approvalStatus, int page,int pageSize)
        {
            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<CapexReportDTO>>(
                    "Invalid page number.","INVALID_PAGE", "Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<CapexReportDTO>>(
                    "Invalid page size.", "INVALID_PAGE_SIZE", "Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"Capex_Status_{approvalStatus}_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<List<CapexReportDTO>> cachedResponse))
            {
                return cachedResponse;
            }

            var capexRequests = await db.CapexRequests
                .Include(x => x.Department)
                .Include(x => x.RequestedByUser)
                .Include(x => x.BudgetLine)
                .Where(x => x.ApprovalStatus == approvalStatus.ToString())
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!capexRequests.Any())
            {
                return ApiResponseHelper.Failure<List<CapexReportDTO>>(
                    "No Capex records found.", "CAPEX_NOT_FOUND", "No Capex requests found for the selected approval status.");
            }

            var result = mapper.Map<List<CapexReportDTO>>(capexRequests);

            var response = ApiResponseHelper.SuccessRes( result, "Capex report fetched successfully.",result.Count);

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }
    }
}
