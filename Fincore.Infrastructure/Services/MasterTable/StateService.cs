using AutoMapper;
using AutoMapper.QueryableExtensions;
using Fincore.Application.CommonHelper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;
using Fincore.Application.DTOs.MasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Fincore.Domain.Models;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.MasterTable
{
    public class StateService : IStateService
    {
        private readonly AppDbContext db;
        private readonly IMapper map;
        private readonly IMemoryCache memoryCache;

        public StateService(AppDbContext db, IMapper map, IMemoryCache memoryCache)
        {
            this.db = db;
            this.map = map;
            this.memoryCache = memoryCache;
        }

        public async Task<ApiResponse<List<StateResponseDto>>> GetAllAsync(int page, int pageSize, string? stateName)
        {
           
            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<StateResponseDto>>(
                    "Invalid page number",
                    "INVALID_PAGE",
                    "Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<StateResponseDto>>(
                    "Invalid page size",
                    "INVALID_PAGE_SIZE",
                    "Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"State_{page}_{pageSize}_{stateName}";

            if (memoryCache.TryGetValue(cacheKey, out List<StateResponseDto>? stateList))
            {
                return ApiResponseHelper.SuccessRes(
                    stateList!,
                    "States fetched successfully",
                    stateList.Count,
                    new { page, pageSize });
            }

            IQueryable<State> query = db.States
                .Include(x => x.Country)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(stateName))
            {
                query = query.Where(x => x.StateName.Contains(stateName));
            }

            stateList = await query
                .OrderBy(x => x.StateId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<StateResponseDto>(map.ConfigurationProvider)
                .ToListAsync();

            if (!stateList.Any())
            {
                return ApiResponseHelper.Failure<List<StateResponseDto>>(
                    "States not found",
                    "EMPTY_DATA",
                    "No data to show");
            }

            memoryCache.Set(cacheKey, stateList, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(
                stateList,
                "States fetched successfully",
                stateList.Count,
                new { page, pageSize });
        }

        public async Task<ApiResponse<StateResponseDto>> GetByIdAsync(int id)
        {
            string cacheKey = $"State_{id}";

            if (memoryCache.TryGetValue(cacheKey, out StateResponseDto? state))
            {
                return ApiResponseHelper.SuccessRes(state!, "State fetched successfully");
            }

            state = await db.States
                .Where(x => x.StateId == id)
                .ProjectTo<StateResponseDto>(map.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (state == null)
            {
                return ApiResponseHelper.Failure<StateResponseDto>(
                    "State not found",
                    "NOT_FOUND",
                    $"State with id {id} not found");
            }

            memoryCache.Set(cacheKey, state, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(state, "State fetched successfully");
        }
        public async Task<ApiResponse<StateResponseDto>> CreateAsync(StateDto dto)
        {
            bool exists = await db.States.AnyAsync(x =>
                x.StateName.ToLower() == dto.StateName.ToLower() &&
                x.CountryId == dto.CountryId);

            if (exists)
            {
                return ApiResponseHelper.Failure<StateResponseDto>(
                    "State already exists",
                    "DUPLICATE_RECORD",
                    "State already exists for this country");
            }

            bool countryExists = await db.Countries.AnyAsync(x => x.CountryId == dto.CountryId);

            if (!countryExists)
            {
                return ApiResponseHelper.Failure<StateResponseDto>(
                    "Country not found",
                    "NOT_FOUND",
                    $"Country with id {dto.CountryId} not found");
            }

            var state = map.Map<State>(dto);

            db.States.Add(state);

            await db.SaveChangesAsync();

            memoryCache.Remove("State");

            var result = await db.States
                .Where(x => x.StateId == state.StateId)
                .ProjectTo<StateResponseDto>(map.ConfigurationProvider)
                .FirstAsync();

            return ApiResponseHelper.SuccessRes(result, "State created successfully");
        }

        public async Task<ApiResponse<StateResponseDto>> UpdateAsync(int id, StateDto dto)
        {
            var state = await db.States.FindAsync(id);

            if (state == null)
            {
                return ApiResponseHelper.Failure<StateResponseDto>(
                    "State not found",
                    "NOT_FOUND",
                    $"State with id {id} not found");
            }

            bool exists = await db.States.AnyAsync(x =>
                x.StateId != id &&
                x.StateName.ToLower() == dto.StateName.ToLower() &&
                x.CountryId == dto.CountryId);

            if (exists)
            {
                return ApiResponseHelper.Failure<StateResponseDto>(
                    "State already exists",
                    "DUPLICATE_RECORD",
                    "State already exists for this country");
            }

            bool countryExists = await db.Countries.AnyAsync(x => x.CountryId == dto.CountryId);

            if (!countryExists)
            {
                return ApiResponseHelper.Failure<StateResponseDto>(
                    "Country not found",
                    "NOT_FOUND",
                    $"Country with id {dto.CountryId} not found");
            }

            state.StateName = dto.StateName;
            state.CountryId = dto.CountryId;

            await db.SaveChangesAsync();

            memoryCache.Remove("State");
            memoryCache.Remove($"State_{id}");

            var result = await db.States
                .Where(x => x.StateId == id)
                .ProjectTo<StateResponseDto>(map.ConfigurationProvider)
                .FirstAsync();

            return ApiResponseHelper.SuccessRes(result, "State updated successfully");
        }

        public async Task<ApiResponse<string>> DeleteAsync(int id)
        {
            var state = await db.States.FindAsync(id);

            if (state == null)
            {
                return ApiResponseHelper.Failure<string>(
                    "Cannot delete state",
                    "NOT_FOUND",
                    $"State with id {id} not found");
            }

            bool cityExists = await db.Cities.AnyAsync(x => x.StateId == id);

            if (cityExists)
            {
                return ApiResponseHelper.Failure<string>(
                    "Cannot delete state",
                    "DELETE_RESTRICTED",
                    $"State with id {id} cannot be deleted because it is linked to City");
            }

            db.States.Remove(state);

            await db.SaveChangesAsync();

            memoryCache.Remove("State");
            memoryCache.Remove($"State_{id}");

            return ApiResponseHelper.SuccessRes($"State deleted successfully with id {id}");
        }
    }
}