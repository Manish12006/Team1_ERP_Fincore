using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTOs.BudgetCategory;
using Fincore.Application.Interfaces.BudgetCategory;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.BudgetCategory
{
    public class BudgetCategoryService : IBudgetCategoryService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public BudgetCategoryService(
            AppDbContext context,
            IMapper mapper,
            IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }
        public async Task<ApiResponse<string>> AddBudgetCategory(CreateBudgetCategoryDTO dto)
        {
            ApiResponse<string> response = new();

            // Department Validation
            var department = await _context.Departments
                .FirstOrDefaultAsync(x => x.DepartmentId == dto.DepartmentId);

            if (department == null)
            {
                response.success = false;
                response.message = "Department Not Found";
                return response;
            }

            // Category Name Validation
            if (string.IsNullOrWhiteSpace(dto.CategoryName))
            {
                response.success = false;
                response.message = "Category Name is required";
                return response;
            }

            if (dto.CategoryName.Length > 20)
            {
                response.success = false;
                response.message = "Category Name should not exceed 20 characters";
                return response;
            }

            // Duplicate Validation
            var exists = await _context.BudgetCategories
                .AnyAsync(x => x.CategoryName == dto.CategoryName && x.IsActive == 1);

            if (exists)
            {
                response.success = false;
                response.message = "Budget Category already exists";
                return response;
            }

            var entity = _mapper.Map<Fincore.Domain.Models.BudgetCategory>(dto);

            entity.IsActive = 1;
            entity.CreatedAt = DateTime.Now;
            entity.ModifiedAt = DateTime.Now;

            await _context.BudgetCategories.AddAsync(entity);
            await _context.SaveChangesAsync();

            _cache.Remove("BudgetCategoryList");

            response.success = true;
            response.message = "Budget Category Added Successfully";
            response.data = "Success";

            return response;
        }
        public async Task<ApiResponse<List<BudgetCategoryResponseDTO>>> GetBudgetCategories(
    string? categoryName,
    int? departmentId,
    byte? isActive,
    int page,
    int pageSize)
        {
            ApiResponse<List<BudgetCategoryResponseDTO>> response =
                new ApiResponse<List<BudgetCategoryResponseDTO>>();

            string cacheKey =
                $"BudgetCategory_{categoryName}_{departmentId}_{isActive}_{page}_{pageSize}";

            if (!_cache.TryGetValue(cacheKey, out List<BudgetCategoryResponseDTO> data))
            {
                var query = _context.BudgetCategories
      .Where(x => x.IsActive == 1)
      .AsQueryable();

                if (!string.IsNullOrEmpty(categoryName))
                    query = query.Where(x => x.CategoryName.Contains(categoryName));

                if (departmentId.HasValue)
                    query = query.Where(x => x.DepartmentId == departmentId);

                if (isActive.HasValue)
                    query = query.Where(x => x.IsActive == isActive);

                var list = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                data = _mapper.Map<List<BudgetCategoryResponseDTO>>(list);

                _cache.Set(cacheKey, data, TimeSpan.FromMinutes(5));
            }

            response.success = true;
            response.message = "Budget Categories Fetched Successfully";
            response.data = data;
            response.totalNumberRecord = data.Count;

            return response;
        }
        public async Task<ApiResponse<BudgetCategoryResponseDTO>> GetBudgetCategoryById(int id)
        {
            ApiResponse<BudgetCategoryResponseDTO> response =
                new ApiResponse<BudgetCategoryResponseDTO>();

            string cacheKey = $"BudgetCategory_{id}";

            if (!_cache.TryGetValue(cacheKey, out BudgetCategoryResponseDTO dto))
            {
                var entity = await _context.BudgetCategories
      .FirstOrDefaultAsync(x =>
          x.BudgetCategoryId == id &&
          x.IsActive == 1);

                if (entity == null)
                {
                    response.success = false;
                    response.message = "Budget Category Not Found";
                    return response;
                }

                dto = _mapper.Map<BudgetCategoryResponseDTO>(entity);

                _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));
            }

            response.success = true;
            response.message = "Budget Category Found Successfully";
            response.data = dto;

            return response;
        }

        public async Task<ApiResponse<string>> UpdateBudgetCategory(int id, UpdateBudgetCategoryDTO dto)
        {
            ApiResponse<string> response = new();

            var entity = await _context.BudgetCategories
                .FirstOrDefaultAsync(x => x.BudgetCategoryId == id);

            if (entity == null)
            {
                response.success = false;
                response.message = "Budget Category Not Found";
                return response;
            }

            // Department Validation
            var department = await _context.Departments
                .FirstOrDefaultAsync(x => x.DepartmentId == dto.DepartmentId);

            if (department == null)
            {
                response.success = false;
                response.message = "Department Not Found";
                return response;
            }

            // Category Name Validation
            if (string.IsNullOrWhiteSpace(dto.CategoryName))
            {
                response.success = false;
                response.message = "Category Name is required";
                return response;
            }

            if (dto.CategoryName.Length > 20)
            {
                response.success = false;
                response.message = "Category Name should not exceed 20 characters";
                return response;
            }

            // Duplicate Validation
            var exists = await _context.BudgetCategories.AnyAsync(x =>
                x.CategoryName == dto.CategoryName &&
                x.BudgetCategoryId != id &&
                x.IsActive == 1);

            if (exists)
            {
                response.success = false;
                response.message = "Budget Category already exists";
                return response;
            }

            _mapper.Map(dto, entity);

            entity.ModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _cache.Remove("BudgetCategoryList");
            _cache.Remove($"BudgetCategory_{id}");

            response.success = true;
            response.message = "Budget Category Updated Successfully";
            response.data = "Success";

            return response;
        }
        public async Task<ApiResponse<string>> DeleteBudgetCategory(int id)
        {
            ApiResponse<string> response = new();

            var entity = await _context.BudgetCategories
                .FirstOrDefaultAsync(x => x.BudgetCategoryId == id);

            if (entity == null)
            {
                response.success = false;
                response.message = "Budget Category Not Found";
                return response;
            }

            entity.IsActive = 0;
            entity.ModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _cache.Remove("BudgetCategoryList");
            _cache.Remove($"BudgetCategory_{id}");

            response.success = true;
            response.message = "Budget Category Deleted Successfully";
            response.data = "Success";

            return response;
        }
        public async Task<ApiResponse<BudgetCategorySummaryDTO>> GetBudgetCategorySummary()
        {
            ApiResponse<BudgetCategorySummaryDTO> response =
                new ApiResponse<BudgetCategorySummaryDTO>();

            BudgetCategorySummaryDTO summary = new BudgetCategorySummaryDTO();

            summary.TotalCategories =
          await _context.BudgetCategories.CountAsync(x => x.IsActive == 1);

            summary.ActiveCategories =
                await _context.BudgetCategories.CountAsync(x => x.IsActive == 1);

            summary.InactiveCategories =
                await _context.BudgetCategories.CountAsync(x => x.IsActive == 0);

            response.success = true;
            response.message = "Budget Category Summary Fetched Successfully";
            response.data = summary;

            return response;
        }
    }
}