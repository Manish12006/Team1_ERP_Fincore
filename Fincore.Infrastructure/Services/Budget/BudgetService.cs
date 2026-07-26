using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTOs.Budget;
using Fincore.Application.Interfaces.Budget;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.Budget
{
    public class BudgetService : IBudgetService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public BudgetService(
            AppDbContext context,
            IMapper mapper,
            IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }
        public async Task<ApiResponse<string>> AddBudget(CreateBudgetDTO dto)
        {
            ApiResponse<string> response = new();
            if (string.IsNullOrWhiteSpace(dto.BudgetCode))
            {
                response.success = false;
                response.message = "Budget Code is required";
                return response;
            }
            if (string.IsNullOrWhiteSpace(dto.BudgetName))
            {
                response.success = false;
                response.message = "Budget Name is required";
                return response;
            }
            if (dto.BudgetName.Length > 30)
            {
                response.success = false;
                response.message = "Budget Name should not exceed 30 characters";
                return response;
            }
            if (dto.BudgetAmount <= 0)
            {
                response.success = false;
                response.message = "Budget Amount must be greater than zero";
                return response;
            }
            if (string.IsNullOrWhiteSpace(dto.FinancialYear))
            {
                response.success = false;
                response.message = "Financial Year is required";
                return response;
            }
            if (dto.StartDate > dto.EndDate)
            {
                response.success = false;
                response.message = "Start Date cannot be greater than End Date";
                return response;
            }
            var budget = await _context.Budgets
    .FirstOrDefaultAsync(x => x.BudgetCode == dto.BudgetCode);

            if (budget != null)
            {
                response.success = false;
                response.message = "Budget Code Already Exists";
                return response;
            }
            var entity = _mapper.Map<Fincore.Domain.Models.Budget>(dto);

            entity.CreatedAt = DateTime.Now;
            entity.ModifiedAt = DateTime.Now;

            await _context.Budgets.AddAsync(entity);

            await _context.SaveChangesAsync();

            _cache.Remove("BudgetList");

            response.success = true;
            response.message = "Budget Added Successfully";
            response.data = "Success";

            return response;
        }
        public async Task<ApiResponse<List<BudgetResponseDTO>>> GetBudgets(
    string? budgetCode,
    string? budgetName,
    string? financialYear,
    int? budgetCategoryId,
    byte? isActive,
    int page,
    int pageSize)
        {
            ApiResponse<List<BudgetResponseDTO>> response =
                new ApiResponse<List<BudgetResponseDTO>>();

            string cacheKey =
                $"Budget_{budgetCode}_{budgetName}_{financialYear}_{budgetCategoryId}_{isActive}_{page}_{pageSize}";

            if (!_cache.TryGetValue(cacheKey, out List<BudgetResponseDTO> data))
            {
                var query = _context.Budgets
    .Where(x => x.IsActive == 1)
    .AsQueryable();

                if (!string.IsNullOrEmpty(budgetCode))
                    query = query.Where(x => x.BudgetCode.Contains(budgetCode));

                if (!string.IsNullOrEmpty(budgetName))
                    query = query.Where(x => x.BudgetName.Contains(budgetName));

                if (!string.IsNullOrEmpty(financialYear))
                    query = query.Where(x => x.FinancialYear == financialYear);

                //if (budgetCategoryId.HasValue)
                //    query = query.Where(x => x.BudgetCategoryId == budgetCategoryId);

                if (isActive.HasValue)
                    query = query.Where(x => x.IsActive == isActive);

                var list = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                data = _mapper.Map<List<BudgetResponseDTO>>(list);

                _cache.Set(cacheKey, data, TimeSpan.FromMinutes(5));
            }

            response.success = true;
            response.message = "Budgets Fetched Successfully";
            response.data = data;
            response.totalNumberRecord = data.Count;

            return response;
        }
        public async Task<ApiResponse<BudgetResponseDTO>> GetBudgetById(int id)
        {
            ApiResponse<BudgetResponseDTO> response =
                new ApiResponse<BudgetResponseDTO>();

            string cacheKey = $"Budget_{id}";

            if (!_cache.TryGetValue(cacheKey, out BudgetResponseDTO dto))
            {
                var entity = await _context.Budgets
     .FirstOrDefaultAsync(x => x.BudgetId == id && x.IsActive == 1);

                if (entity == null)
                {
                    response.success = false;
                    response.message = "Budget Not Found";
                    return response;
                }

                dto = _mapper.Map<BudgetResponseDTO>(entity);

                _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));
            }

            response.success = true;
            response.message = "Budget Found Successfully";
            response.data = dto;

            return response;
        }
        public async Task<ApiResponse<string>> UpdateBudget(int id, UpdateBudgetDTO dto)
        {
            ApiResponse<string> response = new();

            var entity = await _context.Budgets
                .FirstOrDefaultAsync(x => x.BudgetId == id);

            if (string.IsNullOrWhiteSpace(dto.BudgetName))
            {
                response.success = false;
                response.message = "Budget Name is required";
                return response;
            }
            if (dto.BudgetName.Length > 30)
            {
                response.success = false;
                response.message = "Budget Name should not exceed 30 characters";
                return response;
            }
            if (dto.BudgetAmount <= 0)
            {
                response.success = false;
                response.message = "Budget Amount must be greater than zero";
                return response;
            }
            if (string.IsNullOrWhiteSpace(dto.FinancialYear))
            {
                response.success = false;
                response.message = "Financial Year is required";
                return response;
            }
            if (dto.StartDate > dto.EndDate)
            {
                response.success = false;
                response.message = "Start Date cannot be greater than End Date";
                return response;
            }
            if (entity == null)
            {
                response.success = false;
                response.message = "Budget Not Found";
                return response;
            }

            _mapper.Map(dto, entity);

            entity.ModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _cache.Remove("BudgetList");
            _cache.Remove($"Budget_{id}");

            response.success = true;
            response.message = "Budget Updated Successfully";
            response.data = "Success";

            return response;
        }
        public async Task<ApiResponse<string>> DeleteBudget(int id)
        {
            ApiResponse<string> response = new();

            var entity = await _context.Budgets
                .FirstOrDefaultAsync(x => x.BudgetId == id);

            if (entity == null)
            {
                response.success = false;
                response.message = "Budget Not Found";
                return response;
            }
            entity.IsActive = 0;

            entity.ModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _cache.Remove("BudgetList");
            _cache.Remove($"Budget_{id}");

            response.success = true;
            response.message = "Budget Deleted Successfully";
            response.data = "Success";

            return response;
        }
        public async Task<ApiResponse<BudgetSummaryDTO>> GetBudgetSummary()
        {
            ApiResponse<BudgetSummaryDTO> response =
                new ApiResponse<BudgetSummaryDTO>();

            BudgetSummaryDTO summary = new();
            summary.TotalBudgets =
    await _context.Budgets.CountAsync(x => x.IsActive == 1);

            summary.ActiveBudgets =
                await _context.Budgets.CountAsync(x => x.IsActive == 1);

            summary.InactiveBudgets =
                await _context.Budgets.CountAsync(x => x.IsActive == 0);

            summary.TotalBudgetAmount =
                await _context.Budgets
                    .Where(x => x.IsActive == 1)
                    .SumAsync(x => x.BudgetAmount);

            response.success = true;
            response.message = "Budget Summary Fetched Successfully";
            response.data = summary;

            return response;
        }
    }
}