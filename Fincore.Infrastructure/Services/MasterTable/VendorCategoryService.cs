using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Fincore.Domain.Enums;
using Fincore.Domain.Models;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.MasterTable
{
    public class VendorCategoryService : IVendorCategoryService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache cache;
        private static int vendorCategoryCacheVersion = 1;

        public VendorCategoryService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }

        private IQueryable<VendorCategory> GetVendorCategoryQuery()
        {
            return db.VendorCategories
                .Where(x => x.IsActive == (byte)IsActive.Active)
                .Include(x => x.CreatedByUser)
                .Include(x => x.ModifiedByUser);
        }

        public async Task<ApiResponse<List<VendorCategoryDto>>> GetAllVendorCategoriesAsync(
            int pageNumber,
            int pageSize,
            string? search)
        {
            try
            {
                if (pageNumber <= 0)
                {
                    pageNumber = 1;
                }

                if (pageSize <= 0)
                {
                    pageSize = 10;
                }

                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim();
                }

                string cacheKey =
                    $"vendorCategories_{vendorCategoryCacheVersion}_page_{pageNumber}_size_{pageSize}_search_{search}";

                if (cache.TryGetValue(
                    cacheKey,
                    out ApiResponse<List<VendorCategoryDto>> cachedData))
                {
                    Console.WriteLine(
                        "GET ALL VENDOR CATEGORIES - Data returned from CACHE");

                    return cachedData;
                }

                Console.WriteLine(
                    "GET ALL VENDOR CATEGORIES - Data returned from DATABASE");

                IQueryable<VendorCategory> query =
                    GetVendorCategoryQuery();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        x.CategoryName.Contains(search) ||
                        (x.Description != null &&
                         x.Description.Contains(search)));
                }

                int totalRecords = await query.CountAsync();

                List<VendorCategory> vendorCategories =
                    await query
                        .OrderBy(x => x.VendorCategoryId)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                List<VendorCategoryDto> vendorCategoryDtos =
                    mapper.Map<List<VendorCategoryDto>>(
                        vendorCategories);

                var metadata = new
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(
                        (double)totalRecords / pageSize)
                };

                ApiResponse<List<VendorCategoryDto>> response =
                    ApiResponseHelper.SuccessRes(
                        vendorCategoryDtos,
                        "Vendor categories retrieved successfully.",
                        totalRecords,
                        metadata);

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<List<VendorCategoryDto>>(
                    "Failed to retrieve vendor categories.",
                    "VENDOR_CATEGORY_GET_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<VendorCategoryDto>> GetVendorCategoryByIdAsync(
            int id)
        {
            try
            {
                string cacheKey = $"vendorCategory_{id}";

                if (cache.TryGetValue(
                    cacheKey,
                    out ApiResponse<VendorCategoryDto> cachedData))
                {
                    Console.WriteLine(
                        "GET VENDOR CATEGORY BY ID - Data returned from CACHE");

                    return cachedData;
                }

                Console.WriteLine(
                    "GET VENDOR CATEGORY BY ID - Data returned from DATABASE");

                VendorCategory vendorCategory =
                    await GetVendorCategoryQuery()
                        .FirstOrDefaultAsync(
                            x => x.VendorCategoryId == id);

                if (vendorCategory == null)
                {
                    return ApiResponseHelper.Failure<VendorCategoryDto>(
                        "Vendor category not found.",
                        "VENDOR_CATEGORY_NOT_FOUND",
                        $"Vendor category with ID {id} does not exist.");
                }

                VendorCategoryDto vendorCategoryDto =
                    mapper.Map<VendorCategoryDto>(
                        vendorCategory);

                ApiResponse<VendorCategoryDto> response =
                    ApiResponseHelper.SuccessRes(
                        vendorCategoryDto,
                        "Vendor category retrieved successfully.");

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<VendorCategoryDto>(
                    "Failed to retrieve vendor category.",
                    "VENDOR_CATEGORY_GET_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<VendorCategoryDto>> CreateVendorCategoryAsync(
            CreateVendorCategoryDto createVendorCategoryDto)
        {
            try
            {
                VendorCategory existingCategory =
                    await db.VendorCategories
                        .FirstOrDefaultAsync(x =>
                            x.CategoryName.ToLower() ==
                            createVendorCategoryDto.CategoryName.ToLower());

                if (existingCategory != null)
                {
                    return ApiResponseHelper.Failure<VendorCategoryDto>(
                        "Vendor category already exists.",
                        "409",
                        "Vendor category name already exists.");
                }

                bool createdByExists = await db.Users
                    .AnyAsync(x =>
                        x.UserId == createVendorCategoryDto.CreatedBy);

                if (!createdByExists)
                {
                    return ApiResponseHelper.Failure<VendorCategoryDto>(
                        "Created by user not found.",
                        "CREATED_BY_USER_NOT_FOUND",
                        $"User with ID {createVendorCategoryDto.CreatedBy} does not exist.");
                }

                VendorCategory vendorCategory =
                    mapper.Map<VendorCategory>(
                        createVendorCategoryDto);

                vendorCategory.VendorCategoryId = 0;

                vendorCategory.IsActive =
                    (byte)IsActive.Active;

                vendorCategory.CreatedAt =
                    DateTime.UtcNow;

                vendorCategory.ModifiedAt =
                    DateTime.UtcNow;

                vendorCategory.ModifiedBy =
                    createVendorCategoryDto.CreatedBy;

                await db.VendorCategories.AddAsync(
                    vendorCategory);

                await db.SaveChangesAsync();

                vendorCategoryCacheVersion++;

                VendorCategory createdVendorCategory =
                    await GetVendorCategoryQuery()
                        .FirstOrDefaultAsync(x =>
                            x.VendorCategoryId ==
                            vendorCategory.VendorCategoryId);

                VendorCategoryDto vendorCategoryDto =
                    mapper.Map<VendorCategoryDto>(
                        createdVendorCategory);

                ApiResponse<VendorCategoryDto> response =
                    ApiResponseHelper.SuccessRes(
                        vendorCategoryDto,
                        "Vendor category created successfully.");

                string cacheKey =
                    $"vendorCategory_{vendorCategory.VendorCategoryId}";

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<VendorCategoryDto>(
                    "Failed to create vendor category.",
                    "VENDOR_CATEGORY_CREATE_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<VendorCategoryDto>> UpdateVendorCategoryAsync(
            int id,
            UpdateVendorCategoryDto updateVendorCategoryDto)
        {
            try
            {
                VendorCategory vendorCategory =
                    await GetVendorCategoryQuery()
                        .FirstOrDefaultAsync(x =>
                            x.VendorCategoryId == id);

                if (vendorCategory == null)
                {
                    return ApiResponseHelper.Failure<VendorCategoryDto>(
                        "Vendor category not found.",
                        "404",
                        "Invalid Vendor Category Id.");
                }

                VendorCategory existingCategory =
                    await db.VendorCategories
                        .FirstOrDefaultAsync(x =>
                            x.CategoryName.ToLower() ==
                            updateVendorCategoryDto.CategoryName.ToLower() &&
                            x.VendorCategoryId != id);

                if (existingCategory != null)
                {
                    return ApiResponseHelper.Failure<VendorCategoryDto>(
                        "Vendor category already exists.",
                        "409",
                        "Vendor category name already exists.");
                }

                bool modifiedByExists = await db.Users
                    .AnyAsync(x =>
                        x.UserId == updateVendorCategoryDto.ModifiedBy);

                if (!modifiedByExists)
                {
                    return ApiResponseHelper.Failure<VendorCategoryDto>(
                        "Modified by user not found.",
                        "MODIFIED_BY_USER_NOT_FOUND",
                        $"User with ID {updateVendorCategoryDto.ModifiedBy} does not exist.");
                }

                vendorCategory.CategoryName =
                    updateVendorCategoryDto.CategoryName;

                vendorCategory.Description =
                    updateVendorCategoryDto.Description;

                vendorCategory.ModifiedBy =
                    updateVendorCategoryDto.ModifiedBy;

                vendorCategory.ModifiedAt =
                    DateTime.UtcNow;

                await db.SaveChangesAsync();

                vendorCategoryCacheVersion++;

                VendorCategory updatedVendorCategory =
                    await GetVendorCategoryQuery()
                        .FirstOrDefaultAsync(x =>
                            x.VendorCategoryId == id);

                VendorCategoryDto vendorCategoryDto =
                    mapper.Map<VendorCategoryDto>(
                        updatedVendorCategory);

                ApiResponse<VendorCategoryDto> response =
                    ApiResponseHelper.SuccessRes(
                        vendorCategoryDto,
                        "Vendor category updated successfully.");

                string cacheKey =
                    $"vendorCategory_{id}";

                cache.Remove(cacheKey);

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<VendorCategoryDto>(
                    "Failed to update vendor category.",
                    "VENDOR_CATEGORY_UPDATE_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<string>> DeleteVendorCategoryAsync(
            int id)
        {
            try
            {
                VendorCategory vendorCategory =
                    await db.VendorCategories
                        .FirstOrDefaultAsync(x =>
                            x.VendorCategoryId == id);

                if (vendorCategory == null)
                {
                    return ApiResponseHelper.Failure<string>(
                        "Vendor category not found.",
                        "404",
                        "Invalid Vendor Category Id.");
                }

                if (vendorCategory.IsActive ==
                    (byte)IsActive.Inactive)
                {
                    return ApiResponseHelper.Failure<string>(
                        "Vendor category already deleted.",
                        "409",
                        "Vendor category is already inactive.");
                }

                vendorCategory.IsActive =
                    (byte)IsActive.Inactive;

                vendorCategory.ModifiedAt =
                    DateTime.UtcNow;

                vendorCategory.ModifiedBy = 1;

                await db.SaveChangesAsync();

                vendorCategoryCacheVersion++;

                string cacheKey =
                    $"vendorCategory_{id}";

                cache.Remove(cacheKey);

                return ApiResponseHelper.SuccessRes(
                    "Deleted",
                    "Vendor category deleted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<string>(
                    "Failed to delete vendor category.",
                    "VENDOR_CATEGORY_DELETE_ERROR",
                    ex.Message);
            }
        }
    }
}