
using Fincore.Application.CommonHelper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;
using Fincore.Application.DTO.Reports;
using Fincore.Application.Interfaces.IMasterTable;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.MasterTable
{
    public class CurrencyService : ICurrencyService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public CurrencyService(
            AppDbContext context,
            IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<CurrencyResponseDto>>> GetAllAsync(
            int page,
            int pageSize,
            string? currencyName = null)
        {
            string cacheKey =
                $"Currencies_{page}_{pageSize}_{currencyName}";

            if (_cache.TryGetValue(
                cacheKey,
                out ApiResponse<PagedResponse<CurrencyResponseDto>>? cached))
            {
                return cached!;
            }

            var query = _context.Currencies
                .AsQueryable();

            if (!string.IsNullOrEmpty(currencyName))
            {
                query = query.Where(x =>
                    x.CurrencyName.Contains(currencyName));
            }

            int totalRecords = await query.CountAsync();

            if (page < 1)
            {
                return ApiResponseHelper.Failure<PagedResponse<CurrencyResponseDto>>(
                    "Invalid page number.", "INVALID_PAGE", "Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<PagedResponse<CurrencyResponseDto>>(
                "Invalid page number.", "INVALID_PAGE", "Page number must be greater than or equal to 1.");
            }

            var data = await query
                .OrderBy(x => x.CurrencyName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CurrencyResponseDto
                {
                    CurrencyId = x.CurrencyId,
                    CurrencyName = x.CurrencyName,
                    Symbol = x.Symbol
                })
                .ToListAsync();

            var response = new PagedResponse<CurrencyResponseDto>
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
                "Currencies fetched successfully",
                totalRecords);

            _cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(10));

            return result;
        }


        public async Task<ApiResponse<CurrencyResponseDto>> GetByIdAsync(
            int id)
        {
            string cacheKey = $"Currency_{id}";

            if (_cache.TryGetValue(
                cacheKey,
                out ApiResponse<CurrencyResponseDto>? cached))
            {
                return cached!;
            }

            var currency = await _context.Currencies
                .Where(x => x.CurrencyId == id)
                .Select(x => new CurrencyResponseDto
                {
                    CurrencyId = x.CurrencyId,
                    CurrencyName = x.CurrencyName,
                    Symbol = x.Symbol
                })
                .FirstOrDefaultAsync();

            if (currency == null)
            {
                return ApiResponseHelper.Failure<CurrencyResponseDto>(
                    "Currency not found",
                    "NOT_FOUND",
                    "Invalid Currency Id");
            }

            var result = ApiResponseHelper.SuccessRes(
                currency,
                "Currency fetched successfully");

            _cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(10));

            return result;
        }


        public async Task<ApiResponse<CurrencyResponseDto>> CreateAsync(
            CurrencyDto dto)
        {
            bool duplicate = await _context.Currencies
                .AnyAsync(x =>
                    x.CurrencyName.ToLower() ==
                    dto.CurrencyName.ToLower());

            if (duplicate)
            {
                return ApiResponseHelper.Failure<CurrencyResponseDto>(
                    "Currency already exists",
                    "DUPLICATE",
                    "Same currency already exists");
            }

            var currency = new Domain.Models.Currency
            {
                CurrencyName = dto.CurrencyName,
                Symbol = dto.Symbol
            };

            await _context.Currencies.AddAsync(currency);

            await _context.SaveChangesAsync();

            RemoveCache();

            return await GetByIdAsync(currency.CurrencyId);
        }


        public async Task<ApiResponse<CurrencyResponseDto>> UpdateAsync(
            int id,
            CurrencyDto dto)
        {
            var currency = await _context.Currencies
                .FirstOrDefaultAsync(x =>
                    x.CurrencyId == id);

            if (currency == null)
            {
                return ApiResponseHelper.Failure<CurrencyResponseDto>(
                    "Currency not found",
                    "NOT_FOUND",
                    "Invalid Currency Id");
            }

            bool duplicate = await _context.Currencies
                .AnyAsync(x =>
                    x.CurrencyId != id &&
                    x.CurrencyName.ToLower() ==
                    dto.CurrencyName.ToLower());

            if (duplicate)
            {
                return ApiResponseHelper.Failure<CurrencyResponseDto>(
                    "Currency already exists",
                    "DUPLICATE",
                    "Same currency already exists");
            }

            currency.CurrencyName = dto.CurrencyName;
            currency.Symbol = dto.Symbol;

            await _context.SaveChangesAsync();

            _cache.Remove($"Currency_{id}");
            RemoveCache();

            return await GetByIdAsync(id);
        }


        public async Task<ApiResponse<bool>> DeleteAsync(
            int id)
        {
            var currency = await _context.Currencies
                .FirstOrDefaultAsync(x =>
                    x.CurrencyId == id);

            if (currency == null)
            {
                return ApiResponseHelper.Failure<bool>(
                    "Currency not found",
                    "NOT_FOUND",
                    "Invalid Currency Id");
            }

            bool countryExists = await _context.Countries
                .AnyAsync(x =>
                    x.CurrencyId == id);

            if (countryExists)
            {
                return ApiResponseHelper.Failure<bool>(
                    "Currency cannot be deleted",
                    "RESTRICTED",
                    "Currency is assigned to country");
            }

            _context.Currencies.Remove(currency);

            await _context.SaveChangesAsync();

            _cache.Remove($"Currency_{id}");
            RemoveCache();

            return ApiResponseHelper.SuccessRes(
                true,
                "Currency deleted successfully");
        }
        private void RemoveCache()
        {
        }
    }
}