
using Fincore.Application.CommonHelper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Fincore.Domain.Models;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.MasterTable
{
    public class CityService : ICityService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public CityService(
            AppDbContext context,
            IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<CityResponseDto>>> GetAllAsync(
            int page,
            int pageSize,
            string? cityName = null)
        {
            string cacheKey = $"Cities_{page}_{pageSize}_{cityName}";

            if (_cache.TryGetValue(
                cacheKey,
                out ApiResponse<PagedResponse<CityResponseDto>>? cached))
            {
                return cached!;
            }

            var query = _context.Cities
                .Include(x => x.State)
                .AsQueryable();

            if (!string.IsNullOrEmpty(cityName))
            {
                query = query.Where(x =>
                    x.CityName.Contains(cityName));
            }

            int totalRecords = await query.CountAsync();

            if (page < 1)
            {
                return ApiResponseHelper.Failure<PagedResponse<CityResponseDto>>(
                    "Invalid page number.", "INVALID_PAGE", "Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<PagedResponse<CityResponseDto>>(
                "Invalid page number.", "INVALID_PAGE", "Page number must be greater than or equal to 1.");
            }

            var data = await query
                .OrderBy(x => x.CityName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CityResponseDto
                {
                    CityId = x.CityId,
                    CityName = x.CityName,
                    StateId = x.StateId,
                    StateName = x.State.StateName
                })
                .ToListAsync();

            var response = new PagedResponse<CityResponseDto>
            {
                Data = data,
                PageNumber = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(
                    totalRecords / (double)pageSize)
            };

            var result = ApiResponseHelper.SuccessRes(
                response,
                "Cities fetched successfully",
                totalRecords);

            _cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(10));

            return result;
        }

        public async Task<ApiResponse<CityResponseDto>> GetByIdAsync(int id)
        {
            string cacheKey = $"City_{id}";

            if (_cache.TryGetValue(
                cacheKey,
                out ApiResponse<CityResponseDto>? cached))
            {
                return cached!;
            }

            var city = await _context.Cities
                .Include(x => x.State)
                .Where(x => x.CityId == id)
                .Select(x => new CityResponseDto
                {
                    CityId = x.CityId,
                    CityName = x.CityName,
                    StateId = x.StateId,
                    StateName = x.State.StateName
                })
                .FirstOrDefaultAsync();

            if (city == null)
            {
                return ApiResponseHelper.Failure<CityResponseDto>(
                    "City not found",
                    "NOT_FOUND",
                    "Invalid City Id");
            }

            var result = ApiResponseHelper.SuccessRes(
                city,
                "City fetched successfully");

            _cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(10));

            return result;
        }

        public async Task<ApiResponse<CityResponseDto>> CreateAsync(
            CityDto dto)
        {
            bool stateExists = await _context.States
                .AnyAsync(x => x.StateId == dto.StateId);

            if (!stateExists)
            {
                return ApiResponseHelper.Failure<CityResponseDto>(
                    "State not found",
                    "NOT_FOUND",
                    "Invalid State Id");
            }

            bool duplicate = await _context.Cities
                .AnyAsync(x =>
                    x.CityName.ToLower() == dto.CityName.ToLower() &&
                    x.StateId == dto.StateId);

            if (duplicate)
            {
                return ApiResponseHelper.Failure<CityResponseDto>(
                    "City already exists",
                    "DUPLICATE",
                    "Same city already exists in this state");
            }

            var city = new City
            {
                CityName = dto.CityName,
                StateId = dto.StateId
            };

            await _context.Cities.AddAsync(city);
            await _context.SaveChangesAsync();

            RemoveCache();

            return await GetByIdAsync(city.CityId);
        }

        public async Task<ApiResponse<CityResponseDto>> UpdateAsync(
            int id,
            CityDto dto)
        {
            var city = await _context.Cities
                .FirstOrDefaultAsync(x => x.CityId == id);

            if (city == null)
            {
                return ApiResponseHelper.Failure<CityResponseDto>(
                    "City not found",
                    "NOT_FOUND",
                    "Invalid City Id");
            }

            bool stateExists = await _context.States
                .AnyAsync(x => x.StateId == dto.StateId);

            if (!stateExists)
            {
                return ApiResponseHelper.Failure<CityResponseDto>(
                    "State not found",
                    "NOT_FOUND",
                    "Invalid State Id");
            }

            bool duplicate = await _context.Cities
                .AnyAsync(x =>
                    x.CityId != id &&
                    x.CityName.ToLower() == dto.CityName.ToLower() &&
                    x.StateId == dto.StateId);

            if (duplicate)
            {
                return ApiResponseHelper.Failure<CityResponseDto>(
                    "City already exists",
                    "DUPLICATE",
                    "Same city already exists");
            }

            city.CityName = dto.CityName;
            city.StateId = dto.StateId;

            await _context.SaveChangesAsync();

            _cache.Remove($"City_{id}");
            RemoveCache();

            return await GetByIdAsync(id);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var city = await _context.Cities
                .FirstOrDefaultAsync(x => x.CityId == id);

            if (city == null)
            {
                return ApiResponseHelper.Failure<bool>(
                    "City not found",
                    "NOT_FOUND",
                    "Invalid City Id");
            }

            _context.Cities.Remove(city);

            await _context.SaveChangesAsync();

            _cache.Remove($"City_{id}");
            RemoveCache();

            return ApiResponseHelper.SuccessRes(
                true,
                "City deleted successfully");
        }

        private void RemoveCache()
        {
        }
    }
}