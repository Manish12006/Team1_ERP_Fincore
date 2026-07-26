using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Reports;
using Fincore.Application.Interfaces.Reports;
using Fincore.Domain.Enums;
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
    public class BalanceSheetReportService : IBalanceSheetReportService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache memoryCache;

        public BalanceSheetReportService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache memoryCache)
        {
            this.db = db;
            this.mapper = mapper;
            this.memoryCache = memoryCache;
        }

        public async Task<ApiResponse<List<BalanceSheetReportDTO>>> GetBalanceSheetAsync(int page, int pageSize)
        {
            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<BalanceSheetReportDTO>>(
                    "Invalid page number.","INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<BalanceSheetReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE","Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"BalanceSheet_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<List<BalanceSheetReportDTO>> cachedResponse))
            {
                return cachedResponse;
            }

            var journalEntries = await db.JournalEntries
                .Include(x => x.AccountMaster)
                .OrderBy(x => x.EntryDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!journalEntries.Any())
            {
                return ApiResponseHelper.Failure<List<BalanceSheetReportDTO>>(
                    "No balance sheet records found.","BALANCE_SHEET_NOT_FOUND", "No journal entries available.");
            }

            var result = mapper.Map<List<BalanceSheetReportDTO>>(journalEntries);

            var response = ApiResponseHelper.SuccessRes(result,"Balance Sheet report fetched successfully.",result.Count);

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }

        public async Task<ApiResponse<List<BalanceSheetReportDTO>>> GetBalanceSheetByAccountTypeAsync(AccountType accountType,int page, int pageSize)
        {
            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<BalanceSheetReportDTO>>(
                    "Invalid page number.","INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<BalanceSheetReportDTO>>(
                    "Invalid page size.", "INVALID_PAGE_SIZE","Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"BalanceSheet_{accountType}_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<List<BalanceSheetReportDTO>> cachedResponse))
            {
                return cachedResponse;
            }

            var journalEntries = await db.JournalEntries
                .Include(x => x.AccountMaster)
                .Where(x => x.AccountMaster.AccountType == accountType.ToString())
                .OrderBy(x => x.EntryDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!journalEntries.Any())
            {
                return ApiResponseHelper.Failure<List<BalanceSheetReportDTO>>(
                    "No balance sheet records found.","BALANCE_SHEET_NOT_FOUND","No journal entries found for the specified account type.");
            }

            var result = mapper.Map<List<BalanceSheetReportDTO>>(journalEntries);

            var response = ApiResponseHelper.SuccessRes(result, "Balance Sheet report fetched successfully.",result.Count);

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }

        
        public async Task<ApiResponse<List<BalanceSheetReportDTO>>> GetBalanceSheetByDateRangeAsync(DateTime fromDate, DateTime toDate, int page, int pageSize)
        {
            if (fromDate > toDate)
            {
                return ApiResponseHelper.Failure<List<BalanceSheetReportDTO>>(
                    "Invalid date range.","INVALID_DATE_RANGE","From date cannot be greater than To date.");
            }

            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<BalanceSheetReportDTO>>(
                    "Invalid page number.","INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<BalanceSheetReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE","Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"BalanceSheet_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponse<List<BalanceSheetReportDTO>> cachedResponse))
            {
                return cachedResponse;
            }

            var journalEntries = await db.JournalEntries
                .Include(x => x.AccountMaster)
                .Where(x => x.EntryDate.Date >= fromDate.Date &&x.EntryDate.Date <= toDate.Date)
                .OrderBy(x => x.EntryDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!journalEntries.Any())
            {
                return ApiResponseHelper.Failure<List<BalanceSheetReportDTO>>(
                    "No balance sheet records found.","BALANCE_SHEET_NOT_FOUND","No journal entries found for the selected date range.");
            }

            var result = mapper.Map<List<BalanceSheetReportDTO>>(journalEntries);

            var response = ApiResponseHelper.SuccessRes(result,"Balance Sheet report fetched successfully.",result.Count);

            memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }
    }
}
