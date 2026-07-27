using AutoMapper;
using AutoMapper.QueryableExtensions;
using Fincore.Application.DTO;
using Fincore.Application.DTOs.MasterTable;
using Fincore.Application.Interfaces;
using Fincore.Domain.Models;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.MasterTable
{
    public class CountryService : ICountryService
    {
        private readonly AppDbContext db;
        private readonly IMapper map;
        private readonly IMemoryCache memoryCache;

        public CountryService(AppDbContext db, IMapper map, IMemoryCache memoryCache)
        {
            this.db = db;
            this.map = map;
            this.memoryCache = memoryCache;
        }

        public async Task<ApiResponse<List<CountryResponseDto>>> GetAllAsync(
         int page,
        int pageSize,
        string? countryName)
        {
           
            if (page < 1)
            {
                return ApiResponseHelper.Failure<List<CountryResponseDto>>(
                    "Invalid page number",
                    "INVALID_PAGE",
                    "Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<List<CountryResponseDto>>(
                    "Invalid page size",
                    "INVALID_PAGE_SIZE",
                    "Page size must be greater than or equal to 1.");
            }

            string cacheKey = $"Country_{page}_{pageSize}_{countryName}";

            if (memoryCache.TryGetValue(cacheKey, out List<CountryResponseDto>? countryList))
            {
                return ApiResponseHelper.SuccessRes(
                    countryList!,
                    "Countries fetched successfully",
                    countryList.Count,
                    new { page, pageSize });
            }

            IQueryable<Country> query = db.Countries.AsQueryable();

            if (!string.IsNullOrWhiteSpace(countryName))
            {
                query = query.Where(x => x.CountryName.Contains(countryName));
            }

            countryList = await query
                .OrderBy(x => x.CountryId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<CountryResponseDto>(map.ConfigurationProvider)
                .ToListAsync();

            if (!countryList.Any())
            {
                return ApiResponseHelper.Failure<List<CountryResponseDto>>(
                    "Countries not found",
                    "EMPTY_DATA",
                    "No data to show");
            }

            memoryCache.Set(cacheKey, countryList, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(
                countryList,
                "Countries fetched successfully",
                countryList.Count,
                new { page, pageSize });
        }

        public async Task<ApiResponse<CountryResponseDto>> GetByIdAsync(int id)
        {
            string cacheKey = $"Country_{id}";

            if (memoryCache.TryGetValue(cacheKey, out CountryResponseDto? country))
            {
                return ApiResponseHelper.SuccessRes(country!, "Country fetched successfully");
            }

            country = await db.Countries
                .Where(x => x.CountryId == id)
                .ProjectTo<CountryResponseDto>(map.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (country == null)
            {
                return ApiResponseHelper.Failure<CountryResponseDto>(
                    "Country not found",
                    "NOT_FOUND",
                    $"Country with id {id} not found");
            }

            memoryCache.Set(cacheKey, country, TimeSpan.FromMinutes(5));

            return ApiResponseHelper.SuccessRes(country, "Country fetched successfully");
        }
        public async Task<ApiResponse<CountryResponseDto>> CreateAsync(CountryDto dto)
        {
            bool exists = await db.Countries.AnyAsync(x =>
                x.CountryCode == dto.CountryCode ||
                x.CountryName.ToLower() == dto.CountryName.ToLower());

            if (exists)
            {
                return ApiResponseHelper.Failure<CountryResponseDto>(
                    "Country already exists",
                    "DUPLICATE_RECORD",
                    "Country code or country name already exists");
            }

            bool currencyExists = await db.Currencies.AnyAsync(x => x.CurrencyId == dto.CurrencyId);

            if (!currencyExists)
            {
                return ApiResponseHelper.Failure<CountryResponseDto>(
                    "Currency not found",
                    "NOT_FOUND",
                    $"Currency with id {dto.CurrencyId} not found");
            }

            var country = map.Map<Country>(dto);

            db.Countries.Add(country);

            await db.SaveChangesAsync();

            memoryCache.Remove("Country");

            var result = await db.Countries
                .Where(x => x.CountryId == country.CountryId)
                .ProjectTo<CountryResponseDto>(map.ConfigurationProvider)
                .FirstAsync();

            return ApiResponseHelper.SuccessRes(result, "Country created successfully");
        }

        public async Task<ApiResponse<CountryResponseDto>> UpdateAsync(int id, CountryDto dto)
        {
            var country = await db.Countries.FindAsync(id);

            if (country == null)
            {
                return ApiResponseHelper.Failure<CountryResponseDto>(
                    "Country not found",
                    "NOT_FOUND",
                    $"Country with id {id} not found");
            }

            bool exists = await db.Countries.AnyAsync(x =>
                x.CountryId != id &&
                (x.CountryCode == dto.CountryCode ||
                 x.CountryName.ToLower() == dto.CountryName.ToLower()));

            if (exists)
            {
                return ApiResponseHelper.Failure<CountryResponseDto>(
                    "Country already exists",
                    "DUPLICATE_RECORD",
                    "Country code or country name already exists");
            }

            bool currencyExists = await db.Currencies.AnyAsync(x => x.CurrencyId == dto.CurrencyId);

            if (!currencyExists)
            {
                return ApiResponseHelper.Failure<CountryResponseDto>(
                    "Currency not found",
                    "NOT_FOUND",
                    $"Currency with id {dto.CurrencyId} not found");
            }

            country.CountryCode = dto.CountryCode;
            country.CountryName = dto.CountryName;
            country.CurrencyId = dto.CurrencyId;

            await db.SaveChangesAsync();

            memoryCache.Remove("Country");
            memoryCache.Remove($"Country_{id}");

            var result = await db.Countries
                .Where(x => x.CountryId == id)
                .ProjectTo<CountryResponseDto>(map.ConfigurationProvider)
                .FirstAsync();

            return ApiResponseHelper.SuccessRes(result, "Country updated successfully");
        }

        public async Task<ApiResponse<string>> DeleteAsync(int id)
        {
            var country = await db.Countries.FindAsync(id);

            if (country == null)
            {
                return ApiResponseHelper.Failure<string>(
                    "Cannot delete country",
                    "NOT_FOUND",
                    $"Country with id {id} not found");
            }

            bool companyExists = await db.Companies.AnyAsync(x => x.CountryId == id);

            if (companyExists)
            {
                return ApiResponseHelper.Failure<string>(
                    "Cannot delete country",
                    "DELETE_RESTRICTED",
                    $"Country with id {id} cannot be deleted because it is linked to Company");
            }

            bool stateExists = await db.States.AnyAsync(x => x.CountryId == id);

            if (stateExists)
            {
                return ApiResponseHelper.Failure<string>(
                    "Cannot delete country",
                    "DELETE_RESTRICTED",
                    $"Country with id {id} cannot be deleted because it is linked to State");
            }

            db.Countries.Remove(country);

            await db.SaveChangesAsync();

            memoryCache.Remove("Country");
            memoryCache.Remove($"Country_{id}");

            return ApiResponseHelper.SuccessRes($"Country deleted successfully with id {id}");
        }
    }
}
