using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTOs.BudgetLine;
using Fincore.Application.Interfaces.BudgetLine;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.BudgetLine
{
    public class BudgetLineService : IBudgetLineService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public BudgetLineService(
            AppDbContext context,
            IMapper mapper,
            IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }
        public async Task<ApiResponse<string>> AddBudgetLine(CreateBudgetLineDTO dto)
        {
            ApiResponse<string> response = new();

            // Budget Validation
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(x => x.BudgetId == dto.BudgetId);

            if (budget == null)
            {
                response.success = false;
                response.message = "Budget Not Found";
                return response;
            }

            // Budget Category Validation
            var category = await _context.BudgetCategories
                .FirstOrDefaultAsync(x => x.BudgetCategoryId == dto.BudgetCategoryId);

            if (category == null)
            {
                response.success = false;
                response.message = "Budget Category Not Found";
                return response;
            }

            // Amount Validation
            if (dto.AllocatedAmount <= 0)
            {
                response.success = false;
                response.message = "Allocated Amount should be greater than zero";
                return response;
            }

            var entity = _mapper.Map<Fincore.Domain.Models.BudgetLine>(dto);

            entity.CreatedAt = DateTime.Now;
            entity.ModifiedAt = DateTime.Now;

            await _context.BudgetLines.AddAsync(entity);

            await _context.SaveChangesAsync();

            _cache.Remove("BudgetLineList");

            response.success = true;
            response.message = "Budget Line Added Successfully";
            response.data = "Success";

            return response;
        }
        public async Task<ApiResponse<List<BudgetLineResponseDTO>>> GetBudgetLines(
    int? budgetId,
    int? budgetCategoryId,
    byte? isActive,
    int page,
    int pageSize)
        {
            ApiResponse<List<BudgetLineResponseDTO>> response =
                new();

            string cacheKey =
                $"BudgetLine_{budgetId}_{budgetCategoryId}_{isActive}_{page}_{pageSize}";

            if (!_cache.TryGetValue(cacheKey, out List<BudgetLineResponseDTO> data))
            {
                var query = _context.BudgetLines
            .Where(x => x.IsActive == 1)
            .AsQueryable();

                if (budgetId.HasValue)
                    query = query.Where(x => x.BudgetId == budgetId);

                if (budgetCategoryId.HasValue)
                    query = query.Where(x => x.BudgetCategoryId == budgetCategoryId);

                if (isActive.HasValue)
                    query = query.Where(x => x.IsActive == isActive);

                var list = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                data = _mapper.Map<List<BudgetLineResponseDTO>>(list);

                _cache.Set(cacheKey, data, TimeSpan.FromMinutes(5));
            }

            response.success = true;
            response.message = "Budget Lines Fetched Successfully";
            response.data = data;
            response.totalNumberRecord = data.Count;

            return response;
        }
        public async Task<ApiResponse<BudgetLineResponseDTO>> GetBudgetLineById(int id)
        {
            ApiResponse<BudgetLineResponseDTO> response =
                new ApiResponse<BudgetLineResponseDTO>();

            string cacheKey = $"BudgetLine_{id}";

            if (!_cache.TryGetValue(cacheKey, out BudgetLineResponseDTO dto))
            {
                var entity = await _context.BudgetLines
          .FirstOrDefaultAsync(x => x.BudgetLineId == id && x.IsActive == 1);

                if (entity == null)
                {
                    response.success = false;
                    response.message = "Budget Line Not Found";
                    return response;
                }

                dto = _mapper.Map<BudgetLineResponseDTO>(entity);

                _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));
            }

            response.success = true;
            response.message = "Budget Line Found Successfully";
            response.data = dto;

            return response;
        }
        public async Task<ApiResponse<string>> UpdateBudgetLine(int id, UpdateBudgetLineDTO dto)
        {
            ApiResponse<string> response = new();

            var entity = await _context.BudgetLines
                .FirstOrDefaultAsync(x => x.BudgetLineId == id);

            if (entity == null)
            {
                response.success = false;
                response.message = "Budget Line Not Found";
                return response;
            }

            var budget = await _context.Budgets
    .FirstOrDefaultAsync(x => x.BudgetId == dto.BudgetId);

            if (budget == null)
            {
                response.success = false;
                response.message = "Budget Not Found";
                return response;
            }
            var category = await _context.BudgetCategories
    .FirstOrDefaultAsync(x => x.BudgetCategoryId == dto.BudgetCategoryId);

            if (category == null)
            {
                response.success = false;
                response.message = "Budget Category Not Found";
                return response;
            }
            if (dto.AllocatedAmount <= 0)
            {
                response.success = false;
                response.message = "Allocated Amount should be greater than zero";
                return response;
            }
            _mapper.Map(dto, entity);

            entity.ModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _cache.Remove("BudgetLineList");
            _cache.Remove($"BudgetLine_{id}");

            response.success = true;
            response.message = "Budget Line Updated Successfully";
            response.data = "Success";

            return response;
        }
        public async Task<ApiResponse<string>> DeleteBudgetLine(int id)
        {
            ApiResponse<string> response = new();

            var entity = await _context.BudgetLines
                .FirstOrDefaultAsync(x => x.BudgetLineId == id);

            if (entity == null)
            {
                response.success = false;
                response.message = "Budget Line Not Found";
                return response;
            }
            entity.IsActive = 0;
            entity.ModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _cache.Remove("BudgetLineList");
            _cache.Remove($"BudgetLine_{id}");

            response.success = true;
            response.message = "Budget Line Deleted Successfully";
            response.data = "Success";

            return response;
        }
        public async Task<ApiResponse<BudgetLineSummaryDTO>> GetBudgetLineSummary()
        {
            ApiResponse<BudgetLineSummaryDTO> response =
                new ApiResponse<BudgetLineSummaryDTO>();

            BudgetLineSummaryDTO summary = new();

            summary.TotalBudgetLines =
      await _context.BudgetLines
          .CountAsync(x => x.IsActive == 1);

            summary.TotalAllocatedAmount =
     await _context.BudgetLines.SumAsync(x => x.AllocatedAmount);

            summary.TotalUtilizedAmount =
                await _context.BudgetLines
                    .Where(x => x.IsActive == 1)
                    .SumAsync(x => x.UtilizedAmount ?? 0);

            summary.ActiveBudgetLines =
                await _context.BudgetLines.CountAsync(x => x.IsActive == 1);

            summary.InactiveBudgetLines =
                await _context.BudgetLines.CountAsync(x => x.IsActive == 0);

            response.success = true;
            response.message = "Budget Line Summary Fetched Successfully";
            response.data = summary;

            return response;
        }
    }
}