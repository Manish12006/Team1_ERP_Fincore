using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Reports;
using Fincore.Application.Interfaces.Reports;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Google;
using Fincore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Infrastructure.Services.Reports
{

    public class OpexReportService : IOpexReportService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache memoryCache;

        public OpexReportService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache memoryCache)
        {
            this.db = db;
            this.mapper = mapper;
            this.memoryCache = memoryCache;
        }

        public async Task<ApiResponse<List<OpexReportDTO>>> GetOpexAsync(int page, int pageSize)
        {
            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<OpexReportDTO>>(
                    "Invalid page number.","INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<OpexReportDTO>>(
                    "Invalid page size.", "INVALID_PAGE_SIZE","Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"Opex_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<List<OpexReportDTO>> cachedResponse))
            {
                return cachedResponse;
            }

            var opexRequests = await db.OpexRequests
                .Include(x => x.RequestedByUser)
                .Include(x => x.BudgetLine)
                .Include(x => x.ExpenseClaims)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!opexRequests.Any())
            {
                return ApiResponseHelper.Failure<List<OpexReportDTO>>(
                    "No Opex records found.","OPEX_NOT_FOUND", "No Opex requests available.");
            }

            var result = mapper.Map<List<OpexReportDTO>>(opexRequests);

            var response = ApiResponseHelper.SuccessRes(result,"Opex report fetched successfully.",result.Count);

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }

        public async Task<ApiResponse<List<OpexReportDTO>>> GetOpexByStatusAsync( ApprovalStatus approvalStatus,int page,int pageSize)
        {
            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<OpexReportDTO>>(
                    "Invalid page number.", "INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<OpexReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE", "Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"Opex_Status_{approvalStatus}_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<List<OpexReportDTO>> cachedResponse))
            {
                return cachedResponse;
            }

            var opexRequests = await db.OpexRequests
                .Include(x => x.RequestedByUser)
                .Include(x => x.BudgetLine)
                .Include(x => x.ExpenseClaims)
                .Where(x => x.ApprovalStatus == approvalStatus.ToString())
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!opexRequests.Any())
            {
                return ApiResponseHelper.Failure<List<OpexReportDTO>>(
                    "No Opex records found.","OPEX_NOT_FOUND", "No Opex requests found for the selected approval status.");
            }

            var result = mapper.Map<List<OpexReportDTO>>(opexRequests);

            var response = ApiResponseHelper.SuccessRes( result, "Opex report fetched successfully.",result.Count);

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }

        public async Task<ApiResponse<List<OpexReportDTO>>> GetOpexByRequestedByAsync(int requestedBy, int page, int pageSize)
        {
            if (requestedBy <= 0)
            {
                return ApiResponseHelper.Failure<List<OpexReportDTO>>(
                    "Invalid user id.","INVALID_USER_ID","User id must be greater than 0.");
            }

            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<OpexReportDTO>>(
                    "Invalid page number.","INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<OpexReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE","Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"Opex_User_{requestedBy}_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<List<OpexReportDTO>> cachedResponse))
            {
                return cachedResponse;
            }

            var opexRequests = await db.OpexRequests
                .Include(x => x.RequestedByUser)
                .Include(x => x.BudgetLine)
                .Include(x => x.ExpenseClaims)
                .Where(x => x.RequestedBy == requestedBy)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!opexRequests.Any())
            {
                return ApiResponseHelper.Failure<List<OpexReportDTO>>(
                    "No Opex records found.", "OPEX_NOT_FOUND","No Opex requests found for the selected user.");
            }

            var result = mapper.Map<List<OpexReportDTO>>(opexRequests);

            var response = ApiResponseHelper.SuccessRes( result,"Opex report fetched successfully.",result.Count);

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }
    }
}
