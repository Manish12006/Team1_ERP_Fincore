using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.GeneralLedger;
using Fincore.Application.Interfaces;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Infrastructure.Services
{

    public class GeneralLedgerService : IGeneralLedgerService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache memoryCache;

        public GeneralLedgerService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache memoryCache)
        {
            this.db = db;
            this.mapper = mapper;
            this.memoryCache = memoryCache;
        }

        public async Task<ApiResponse<List<GeneralLedgerReadDTO>>> GetAllAsync( string? journalNumber,int? accountId,DateTime? fromDate,DateTime? toDate,string? description,int page,int pageSize)
        {
            if (page <= 0)
            {
                return ApiResponseHelper.Failure<List<GeneralLedgerReadDTO>>(
                    "Invalid Page", "INVALID_PAGE","Page number must be greater than zero.");
            }

            if (pageSize <= 0)
            {
                return ApiResponseHelper.Failure<List<GeneralLedgerReadDTO>>(
                    "Invalid Page Size",  "INVALID_PAGE_SIZE","Page size must be greater than zero.");
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return ApiResponseHelper.Failure<List<GeneralLedgerReadDTO>>(
                    "Invalid Date Range", "INVALID_DATE_RANGE", "From Date cannot be greater than To Date.");
            }

            string cacheKey =
                $"GeneralLedger_{journalNumber}_{accountId}_{fromDate}_{toDate}_{description}_{page}_{pageSize}";

            List<GeneralLedgerReadDTO> ledgerList;

            if (!memoryCache.TryGetValue(cacheKey, out ledgerList))
            {
                var query = db.JournalEntries
                    .Include(x => x.AccountMaster)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(journalNumber))
                {
                    query = query.Where(x => x.JournalNumber.Contains(journalNumber));
                }

                if (accountId.HasValue)
                {
                    query = query.Where(x => x.AccountId == accountId.Value);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.EntryDate.Date >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(x => x.EntryDate.Date <= toDate.Value.Date);
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    query = query.Where(x => x.Description.Contains(description));
                }

                var data = await query
                    .OrderByDescending(x => x.EntryDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ledgerList = mapper.Map<List<GeneralLedgerReadDTO>>(data);

                memoryCache.Set(cacheKey, ledgerList, TimeSpan.FromMinutes(5));
            }

            if (!ledgerList.Any())
            {
                return ApiResponseHelper.Failure<List<GeneralLedgerReadDTO>>(
                    "General Ledger Not Found","GENERAL_LEDGER_NOT_FOUND","No General Ledger Records Found");
            }

            return ApiResponseHelper.SuccessRes(
                ledgerList,
                "General Ledger Fetched Successfully",
                ledgerList.Count,
                new
                {
                    journalNumber,
                    accountId,
                    fromDate,
                    toDate,
                    description,
                    page,
                    pageSize
                });
        }

        public async Task<ApiResponse<GeneralLedgerReadDTO>> GetByIdAsync(int id)
        {
            string cacheKey = $"GeneralLedger_{id}";

            GeneralLedgerReadDTO ledger;

            if (!memoryCache.TryGetValue(cacheKey, out ledger))
            {
                var data = await db.JournalEntries
                    .Include(x => x.AccountMaster)
                    .FirstOrDefaultAsync(x => x.JournalEntryId == id);

                if (data != null)
                {
                    ledger = mapper.Map<GeneralLedgerReadDTO>(data);
                    memoryCache.Set(cacheKey, ledger, TimeSpan.FromMinutes(5));
                }
            }

            if (ledger == null)
            {
                return ApiResponseHelper.Failure<GeneralLedgerReadDTO>(
                    "General Ledger Record Not Found","GENERAL_LEDGER_NOT_FOUND", $"No General Ledger record found with Id : {id}");
            }

            return ApiResponseHelper.SuccessRes( ledger, "General Ledger Record Fetched Successfully");
        }

        public async Task<ApiResponse<GeneralLedgerSummaryDTO>> GetSummaryAsync()
        {
            const string cacheKey = "GeneralLedgerSummary";

            GeneralLedgerSummaryDTO summary;

            if (!memoryCache.TryGetValue(cacheKey, out summary))
            {
                var journalEntries = await db.JournalEntries.ToListAsync();

                if (journalEntries.Any())
                {
                    summary = new GeneralLedgerSummaryDTO
                    {
                        TotalDebit = journalEntries.Sum(x => x.DebitAmount ?? 0),
                        TotalCredit = journalEntries.Sum(x => x.CreditAmount ?? 0),
                        TotalTransactions = journalEntries.Count
                    };

                    memoryCache.Set(cacheKey, summary, TimeSpan.FromMinutes(5));
                }
            }

            if (summary == null)
            {
                return ApiResponseHelper.Failure<GeneralLedgerSummaryDTO>(
                    "General Ledger Summary Not Found", "GENERAL_LEDGER_SUMMARY_NOT_FOUND","No General Ledger Records Found");
            }

            return ApiResponseHelper.SuccessRes(
                summary,
                "General Ledger Summary Fetched Successfully");
        }
        public async Task<ApiResponse<List<TrialBalanceReadDTO>>> GetTrialBalanceAsync()
        {
            const string cacheKey = "TrialBalance";

            List<TrialBalanceReadDTO> trialBalance;

            if (!memoryCache.TryGetValue(cacheKey, out trialBalance))
            {
                trialBalance = await db.JournalEntries
                    .Include(x => x.AccountMaster)
                    .GroupBy(x => new
                    {
                        x.AccountId,
                        x.AccountMaster.AccountCode,
                        x.AccountMaster.AccountName
                    })
                    .Select(x => new TrialBalanceReadDTO
                    {
                        AccountId = x.Key.AccountId,
                        AccountCode = x.Key.AccountCode,
                        AccountName = x.Key.AccountName,
                        TotalDebit = x.Sum(a => a.DebitAmount ?? 0),
                        TotalCredit = x.Sum(a => a.CreditAmount ?? 0)
                    })
                    .ToListAsync();

                memoryCache.Set(cacheKey, trialBalance, TimeSpan.FromMinutes(5));
            }

            if (!trialBalance.Any())
            {
                return ApiResponseHelper.Failure<List<TrialBalanceReadDTO>>(
                    "Trial Balance Not Found","TRIAL_BALANCE_NOT_FOUND","No Trial Balance Records Found");
            }

            return ApiResponseHelper.SuccessRes( trialBalance, "Trial Balance Fetched Successfully", trialBalance.Count);
        }

        public async Task<ApiResponse<TrialBalanceSummaryDTO>> GetTrialBalanceSummaryAsync()
        {
            const string cacheKey = "TrialBalanceSummary";

            TrialBalanceSummaryDTO summary;

            if (!memoryCache.TryGetValue(cacheKey, out summary))
            {
                var journalEntries = await db.JournalEntries.ToListAsync();

                if (!journalEntries.Any())
                {
                    return ApiResponseHelper.Failure<TrialBalanceSummaryDTO>(
                        "Trial Balance Summary Not Found", "TRIAL_BALANCE_SUMMARY_NOT_FOUND", "No Trial Balance Records Found");
                }

                summary = new TrialBalanceSummaryDTO
                {
                    TotalDebit = journalEntries.Sum(x => x.DebitAmount ?? 0),
                    TotalCredit = journalEntries.Sum(x => x.CreditAmount ?? 0),
                    IsBalanced = journalEntries.Sum(x => x.DebitAmount ?? 0) ==
                                 journalEntries.Sum(x => x.CreditAmount ?? 0)
                };

                memoryCache.Set(cacheKey, summary, TimeSpan.FromMinutes(5));
            }

            return ApiResponseHelper.SuccessRes( summary, "Trial Balance Summary Fetched Successfully");
        }


        public async Task<ApiResponse<List<LedgerAccountReadDTO>>> GetLedgerAccountAsync(int accountId,DateTime? fromDate,DateTime? toDate,int page, int pageSize)
        {
            if (page <= 0)
            {
                return ApiResponseHelper.Failure<List<LedgerAccountReadDTO>>(
                    "Invalid Page","INVALID_PAGE","Page number must be greater than zero.");
            }

            if (pageSize <= 0)
            {
                return ApiResponseHelper.Failure<List<LedgerAccountReadDTO>>(
                    "Invalid Page Size","INVALID_PAGE_SIZE","Page size must be greater than zero.");
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return ApiResponseHelper.Failure<List<LedgerAccountReadDTO>>(
                    "Invalid Date Range","INVALID_DATE_RANGE","From Date cannot be greater than To Date.");
            }

            string cacheKey =
                $"LedgerAccount_{accountId}_{fromDate}_{toDate}_{page}_{pageSize}";

            List<LedgerAccountReadDTO> ledgerList;

            if (!memoryCache.TryGetValue(cacheKey, out ledgerList))
            {
                var query = db.JournalEntries
                    .Include(x => x.AccountMaster)
                    .Where(x => x.AccountId == accountId)
                    .AsQueryable();

                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.EntryDate.Date >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(x => x.EntryDate.Date <= toDate.Value.Date);
                }

                var data = await query
                    .OrderByDescending(x => x.EntryDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ledgerList = mapper.Map<List<LedgerAccountReadDTO>>(data);

                memoryCache.Set(cacheKey, ledgerList, TimeSpan.FromMinutes(5));
            }

            if (!ledgerList.Any())
            {
                return ApiResponseHelper.Failure<List<LedgerAccountReadDTO>>(
                    "Ledger Account Not Found","LEDGER_ACCOUNT_NOT_FOUND", $"No ledger records found for Account Id : {accountId}");
            }

            return ApiResponseHelper.SuccessRes(
                ledgerList,
                "Ledger Account Fetched Successfully",
                ledgerList.Count,
                new
                {
                    accountId,
                    fromDate,
                    toDate,
                    page,
                    pageSize
                });
        }

        public async Task<ApiResponse<List<AccountingReportReadDTO>>> GetAccountingReportAsync(DateTime? fromDate,DateTime? toDate,int? accountId,int page,int pageSize)
        {
            if (page <= 0)
            {
                return ApiResponseHelper.Failure<List<AccountingReportReadDTO>>(
                    "Invalid Page", "INVALID_PAGE", "Page number must be greater than zero.");
            }

            if (pageSize <= 0)
            {
                return ApiResponseHelper.Failure<List<AccountingReportReadDTO>>(
                    "Invalid Page Size","INVALID_PAGE_SIZE", "Page size must be greater than zero.");
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return ApiResponseHelper.Failure<List<AccountingReportReadDTO>>(
                    "Invalid Date Range", "INVALID_DATE_RANGE", "From Date cannot be greater than To Date.");
            }

            string cacheKey = $"AccountingReport_{fromDate}_{toDate}_{accountId}_{page}_{pageSize}";

            List<AccountingReportReadDTO> reportList;

            if (!memoryCache.TryGetValue(cacheKey, out reportList))
            {
                var query = db.JournalEntries
                    .Include(x => x.AccountMaster)
                    .AsQueryable();

                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.EntryDate.Date >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(x => x.EntryDate.Date <= toDate.Value.Date);
                }

                if (accountId.HasValue)
                {
                    query = query.Where(x => x.AccountId == accountId.Value);
                }

                var data = await query
                    .OrderByDescending(x => x.EntryDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                reportList = mapper.Map<List<AccountingReportReadDTO>>(data);

                memoryCache.Set(cacheKey, reportList, TimeSpan.FromMinutes(5));
            }

            if (!reportList.Any())
            {
                return ApiResponseHelper.Failure<List<AccountingReportReadDTO>>(
                    "Accounting Report Not Found", "ACCOUNTING_REPORT_NOT_FOUND", "No Accounting Report Records Found");
            }

            return ApiResponseHelper.SuccessRes(
                reportList,
                "Accounting Report Fetched Successfully",
                reportList.Count,
                new
                {
                    fromDate,
                    toDate,
                    accountId,
                    page,
                    pageSize
                });
        }
    }
}
