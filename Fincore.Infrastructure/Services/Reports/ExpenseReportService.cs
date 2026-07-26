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
    public class ExpenseReportService : IExpenseReportService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache memoryCache;

        public ExpenseReportService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache memoryCache)
        {
            this.db = db;
            this.mapper = mapper;
            this.memoryCache = memoryCache;
        }

        public async Task<ApiResponse<List<ExpenseReportDTO>>> GetExpenseAsync(int page, int pageSize)
        {
            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid page number.", "INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE","Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"ExpenseReport_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out List<ExpenseReportDTO> cachedExpense))
            {
                return ApiResponseHelper.SuccessRes(cachedExpense,"Expense report fetched successfully from cache.",cachedExpense.Count);
            }

            var expense = await db.ExpenseClaims
                .Include(x => x.OpexRequest) .ThenInclude(x => x.BudgetLine).ThenInclude(x => x.BudgetCategory).ThenInclude(x => x.Department)
                .Include(x => x.ClaimByUser)
                .OrderByDescending(x => x.ExpenseDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!expense.Any())
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "No expense records found.","EXPENSE_NOT_FOUND","No expense records are available.");
            }

            var result = mapper.Map<List<ExpenseReportDTO>>(expense);

            memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(
                result,"Expense report fetched successfully.",result.Count);
        }

        public async Task<ApiResponse<List<ExpenseReportDTO>>> GetExpenseByEmployeeAsync(int employeeId, int page, int pageSize)
        {
            if (employeeId <= 0)
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid employee ID.","INVALID_EMPLOYEE_ID","Employee ID must be greater than 0.");
            }

            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid page number.", "INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid page size.", "INVALID_PAGE_SIZE", "Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"ExpenseEmployee_{employeeId}_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out List<ExpenseReportDTO> cachedExpense))
            {
                return ApiResponseHelper.SuccessRes( cachedExpense,"Employee expense report fetched successfully from cache.",cachedExpense.Count);
            }

            var expense = await db.ExpenseClaims
                .Include(x => x.OpexRequest).ThenInclude(x => x.BudgetLine).ThenInclude(x => x.BudgetCategory).ThenInclude(x => x.Department)
                .Include(x => x.ClaimByUser)
                .Where(x => x.ClaimBy == employeeId)
                .OrderByDescending(x => x.ExpenseDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!expense.Any())
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "No expense records found.","EXPENSE_NOT_FOUND","No expense records found for the specified employee.");
            }

            var result = mapper.Map<List<ExpenseReportDTO>>(expense);

            memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(
                result,"Employee expense report fetched successfully.",
                result.Count);
        }

        public async Task<ApiResponse<List<ExpenseReportDTO>>> GetExpenseByExpenseTypeAsync(ExpenseType expenseType, int page, int pageSize)
        {
            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid page number.", "INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid page size.", "INVALID_PAGE_SIZE","Page size must be greater than or equal to 1.");
            }

            if (!Enum.IsDefined(typeof(ExpenseType), expenseType))
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid expense type.","INVALID_EXPENSE_TYPE","The specified expense type is not valid.");
            }

            string expenseTypeValue = expenseType switch
            {
                ExpenseType.ClientEntertainment => "Client Entertainment",
                ExpenseType.OfficeSupplies => "Office Supplies", _ => expenseType.ToString()
            };

            string cacheKey = $"ExpenseType_{expenseType}_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out List<ExpenseReportDTO> cachedExpense))
            {
                return ApiResponseHelper.SuccessRes(cachedExpense,"Expense report fetched successfully from cache.",cachedExpense.Count);
            }

            var expense = await db.ExpenseClaims
                .Include(x => x.OpexRequest).ThenInclude(x => x.BudgetLine).ThenInclude(x => x.BudgetCategory).ThenInclude(x => x.Department)
                .Include(x => x.ClaimByUser)
                .Where(x => x.ExpenseType == expenseTypeValue)
                .OrderByDescending(x => x.ExpenseDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!expense.Any())
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "No expense records found.","EXPENSE_NOT_FOUND","No expense records found for the specified expense type.");
            }

            var result = mapper.Map<List<ExpenseReportDTO>>(expense);

            memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes( result,"Expense report fetched successfully.",result.Count);
        }

        public async Task<ApiResponse<List<ExpenseReportDTO>>> GetExpenseByStatusAsync(ApprovalStatus status, int page, int pageSize)
        {
            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid page number.","INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE", "Page size must be greater than or equal to 1.");
            }

            if (!Enum.IsDefined(typeof(ApprovalStatus), status))
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid approval status.","INVALID_APPROVAL_STATUS","The specified approval status is not valid.");
            }

            string cacheKey = $"ExpenseStatus_{status}_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out List<ExpenseReportDTO> cachedExpense))
            {
                return ApiResponseHelper.SuccessRes( cachedExpense,"Expense report fetched successfully from cache.",cachedExpense.Count);
            }

            var expense = await db.ExpenseClaims
                .Include(x => x.OpexRequest).ThenInclude(x => x.BudgetLine).ThenInclude(x => x.BudgetCategory) .ThenInclude(x => x.Department)
                .Include(x => x.ClaimByUser)
                .Where(x => x.ApprovalStatus == status.ToString())
                .OrderByDescending(x => x.ExpenseDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!expense.Any())
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "No expense records found.", "EXPENSE_NOT_FOUND","No expense records found for the specified approval status.");
            }

            var result = mapper.Map<List<ExpenseReportDTO>>(expense);

            memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes( result,"Expense report fetched successfully.",result.Count);
        }

        public async Task<ApiResponse<List<ExpenseReportDTO>>> GetExpenseByDateRangeAsync(DateTime fromDate, DateTime toDate, int page, int pageSize)
        {
            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid page number.","INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid page size.", "INVALID_PAGE_SIZE","Page size must be greater than or equal to 1.");
            }

            if (fromDate == DateTime.MinValue || toDate == DateTime.MinValue)
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid date.","INVALID_DATE","Please provide valid from and to dates.");
            }

            if (fromDate > toDate)
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid date range.","INVALID_DATE_RANGE","From date cannot be greater than To date.");
            }

            string cacheKey = $"ExpenseDateRange_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out List<ExpenseReportDTO> cachedExpense))
            {
                return ApiResponseHelper.SuccessRes( cachedExpense,"Expense report fetched successfully from cache.",cachedExpense.Count);
            }

            var expense = await db.ExpenseClaims
                .Include(x => x.OpexRequest).ThenInclude(x => x.BudgetLine).ThenInclude(x => x.BudgetCategory).ThenInclude(x => x.Department)
                .Include(x => x.ClaimByUser)
                .Where(x => x.ExpenseDate >= fromDate && x.ExpenseDate <= toDate)
                .OrderByDescending(x => x.ExpenseDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!expense.Any())
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "No expense records found.","EXPENSE_NOT_FOUND","No expense records found for the specified date range.");
            }

            var result = mapper.Map<List<ExpenseReportDTO>>(expense);

            memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes( result,"Expense report fetched successfully.",result.Count);
        }

        public async Task<ApiResponse<List<ExpenseReportDTO>>> GetExpenseByOpexRequestAsync(int opexRequestId, int page, int pageSize)
        {
            if (opexRequestId <= 0)
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid Opex Request ID.","INVALID_OPEX_REQUEST_ID","Opex Request ID must be greater than 0.");
            }

            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid page number.","INVALID_PAGE","Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "Invalid page size.","INVALID_PAGE_SIZE","Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"ExpenseOpex_{opexRequestId}_{page}_{pageSize}";

            if (memoryCache.TryGetValue(cacheKey, out List<ExpenseReportDTO> cachedExpense))
            {
                return ApiResponseHelper.SuccessRes(cachedExpense,"Expense report fetched successfully from cache.",cachedExpense.Count);
            }

            var expense = await db.ExpenseClaims
                .Include(x => x.OpexRequest).ThenInclude(x => x.BudgetLine).ThenInclude(x => x.BudgetCategory) .ThenInclude(x => x.Department)
                .Include(x => x.ClaimByUser)
                .Where(x => x.OpexRequestId == opexRequestId)
                .OrderByDescending(x => x.ExpenseDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!expense.Any())
            {
                return ApiResponseHelper.Failure<List<ExpenseReportDTO>>(
                    "No expense records found.","EXPENSE_NOT_FOUND","No expense records found for the specified Opex Request.");
            }

            var result = mapper.Map<List<ExpenseReportDTO>>(expense);

            memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(result,"Expense report fetched successfully.",result.Count);
        }
    }
}
