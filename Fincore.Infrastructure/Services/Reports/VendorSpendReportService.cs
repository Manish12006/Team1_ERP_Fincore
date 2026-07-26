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
    public class VendorSpendReportService : IVendorSpendReportService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache memoryCache;

        public VendorSpendReportService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache memoryCache)
        {
            this.db = db;
            this.mapper = mapper;
            this.memoryCache = memoryCache;
        }

        public async Task<ApiResponse<List<VendorSpendDTO>>> GetVendorSpendAsync(int page, int pageSize)
        {
            if (page < 1)
                return ApiResponseHelper.Failure<List<VendorSpendDTO>>(
                    "Invalid page number.","INVALID_PAGE","Page number must be greater than or equal to 1.");

            if (pageSize < 1)
                return ApiResponseHelper.Failure<List<VendorSpendDTO>>(
                    "Invalid page size.", "INVALID_PAGE_SIZE","Page size must be greater than or equal to 1.");

            string cacheKey = $"VendorSpend_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<List<VendorSpendDTO>> cached))
                return cached;

            var payments = await db.Payments
                .Include(x => x.Vendor).ThenInclude(x => x.Company)
                .Include(x => x.APInvoice)
                .OrderByDescending(x => x.PaymentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!payments.Any())
                return ApiResponseHelper.Failure<List<VendorSpendDTO>>(
                    "No Vendor Spend records found.","NOT_FOUND","No records available.");

            var result = mapper.Map<List<VendorSpendDTO>>(payments);

            var response = ApiResponseHelper.SuccessRes(result,"Vendor Spend Report fetched successfully.",result.Count);

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }

        public async Task<ApiResponse<List<VendorSpendDTO>>> GetVendorSpendByVendorAsync(int vendorId, int page, int pageSize)
        {
            if (vendorId <= 0)
                return ApiResponseHelper.Failure<List<VendorSpendDTO>>(
                    "Invalid Vendor Id.","INVALID_VENDOR", "Vendor Id must be greater than 0.");

            if (page < 1 || pageSize < 1)
                return ApiResponseHelper.Failure<List<VendorSpendDTO>>(
                    "Invalid pagination.","INVALID_REQUEST","Invalid page or page size.");

            string cacheKey = $"VendorSpend_Vendor_{vendorId}_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<List<VendorSpendDTO>> cached))
                return cached;

            var payments = await db.Payments
                .Include(x => x.Vendor).ThenInclude(x => x.Company)
                .Include(x => x.APInvoice)
                .Where(x => x.VendorId == vendorId)
                .OrderByDescending(x => x.PaymentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!payments.Any())
                return ApiResponseHelper.Failure<List<VendorSpendDTO>>(
                    "No records found.","NOT_FOUND", "Vendor not found.");

            var result = mapper.Map<List<VendorSpendDTO>>(payments);

            var response = ApiResponseHelper.SuccessRes( result,"Vendor Spend Report fetched successfully.",result.Count);

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }

        public async Task<ApiResponse<List<VendorSpendDTO>>> GetVendorSpendByPaymentStatusAsync(PaymentStatus paymentStatus,int page,int pageSize)
        {
            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<VendorSpendDTO>>(
                    "Invalid page number.", "INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<VendorSpendDTO>>(
                    "Invalid page size.", "INVALID_PAGE_SIZE","Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"VendorSpend_PaymentStatus_{paymentStatus}_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<List<VendorSpendDTO>> cached))
                return cached;

            var payments = await db.Payments
                .Include(x => x.Vendor).ThenInclude(x => x.Company)
                .Include(x => x.APInvoice)
                .Where(x => x.APInvoice.PaymentStatus == paymentStatus.ToString())
                .OrderByDescending(x => x.PaymentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!payments.Any())
            {
                return ApiResponseHelper.Failure<List<VendorSpendDTO>>(
                    "No records found.", "NOT_FOUND","No matching records.");
            }

            var result = mapper.Map<List<VendorSpendDTO>>(payments);

            var response = ApiResponseHelper.SuccessRes( result, "Vendor Spend Report fetched successfully.", result.Count);

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }

        public async Task<ApiResponse<List<VendorSpendDTO>>> GetVendorSpendByApprovalStatusAsync(ApprovalStatus approvalStatus,int page, int pageSize)
        {
            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<VendorSpendDTO>>(
                    "Invalid page number.","INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<VendorSpendDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE","Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"VendorSpend_Approval_{approvalStatus}_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<List<VendorSpendDTO>> cached))
                return cached;

            var payments = await db.Payments
                .Include(x => x.Vendor) .ThenInclude(x => x.Company)
                .Include(x => x.APInvoice)
                .Where(x => x.ApprovalStatus == approvalStatus.ToString())
                .OrderByDescending(x => x.PaymentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!payments.Any())
            {
                return ApiResponseHelper.Failure<List<VendorSpendDTO>>(
                    "No records found.", "NOT_FOUND","No matching records.");
            }

            var result = mapper.Map<List<VendorSpendDTO>>(payments);

            var response = ApiResponseHelper.SuccessRes( result,"Vendor Spend Report fetched successfully.", result.Count);

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }

        public async Task<ApiResponse<List<VendorSpendDTO>>> GetVendorSpendByDateRangeAsync(DateTime fromDate, DateTime toDate, int page, int pageSize)
        {
            if (fromDate > toDate)
                return ApiResponseHelper.Failure<List<VendorSpendDTO>>("Invalid Date Range.","INVALID_DATE","From Date cannot be greater than To Date.");

            string cacheKey = $"VendorSpend_Date_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<List<VendorSpendDTO>> cached))
                return cached;

            var payments = await db.Payments
                .Include(x => x.Vendor) .ThenInclude(x => x.Company)
                .Include(x => x.APInvoice)
                .Where(x => x.PaymentDate >= fromDate && x.PaymentDate <= toDate)
                .OrderByDescending(x => x.PaymentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!payments.Any())
                return ApiResponseHelper.Failure<List<VendorSpendDTO>>(
                    "No records found.","NOT_FOUND","No matching records.");

            var result = mapper.Map<List<VendorSpendDTO>>(payments);

            var response = ApiResponseHelper.SuccessRes( result,"Vendor Spend Report fetched successfully.",result.Count);

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }
    }
}
