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
    public class VendorService : IVendorService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache cache;
        private static int vendorCacheVersion = 1;

        public VendorService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }

        private IQueryable<Vendor> GetVendorQuery()
        {
            return db.Vendors
                .Where(x => x.IsActive == (byte)IsActive.Active)
                .Include(x => x.VendorCategory)
                .Include(x => x.Company)
                .Include(x => x.CreatedByUser)
                .Include(x => x.ModifiedByUser);
        }

        public async Task<ApiResponse<List<VendorDto>>> GetAllVendorsAsync(
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
                    $"vendors_{vendorCacheVersion}_page_{pageNumber}_size_{pageSize}_search_{search}";

                if (cache.TryGetValue(
                    cacheKey,
                    out ApiResponse<List<VendorDto>> cachedData))
                {
                    Console.WriteLine(
                        "GET ALL VENDORS - Data returned from CACHE");

                    return cachedData;
                }

                Console.WriteLine(
                    "GET ALL VENDORS - Data returned from DATABASE");

                IQueryable<Vendor> query = GetVendorQuery();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        x.VendorCode.Contains(search) ||
                        x.PAN.Contains(search) ||
                        x.VendorCategory.CategoryName.Contains(search) ||
                        x.Company.CompanyName.Contains(search));
                }

                int totalRecords = await query.CountAsync();

                List<Vendor> vendors = await query
                    .OrderBy(x => x.VendorId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                List<VendorDto> vendorDtos =
                    mapper.Map<List<VendorDto>>(vendors);

                var metadata = new
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(
                        (double)totalRecords / pageSize)
                };

                ApiResponse<List<VendorDto>> response =
                    ApiResponseHelper.SuccessRes(
                        vendorDtos,
                        "Vendors fetched successfully.",
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
                return ApiResponseHelper.Failure<List<VendorDto>>(
                    "Failed to retrieve vendors.",
                    "VENDOR_GET_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<VendorDto>> GetVendorByIdAsync(
            int id)
        {
            try
            {
                string cacheKey = $"vendor_{id}";

                if (cache.TryGetValue(
                    cacheKey,
                    out ApiResponse<VendorDto> cachedData))
                {
                    Console.WriteLine(
                        "GET VENDOR BY ID - Data returned from CACHE");

                    return cachedData;
                }

                Console.WriteLine(
                    "GET VENDOR BY ID - Data returned from DATABASE");

                Vendor vendor = await GetVendorQuery()
                    .FirstOrDefaultAsync(x => x.VendorId == id);

                if (vendor == null)
                {
                    return ApiResponseHelper.Failure<VendorDto>(
                        "Vendor not found.",
                        "VENDOR_NOT_FOUND",
                        $"Vendor with ID {id} does not exist.");
                }

                VendorDto vendorDto =
                    mapper.Map<VendorDto>(vendor);

                ApiResponse<VendorDto> response =
                    ApiResponseHelper.SuccessRes(
                        vendorDto,
                        "Vendor fetched successfully.");

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<VendorDto>(
                    "Failed to retrieve vendor.",
                    "VENDOR_GET_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<VendorDto>> CreateVendorAsync(
            CreateVendorDto createVendorDto)
        {
            try
            {
                bool vendorCategoryExists =
                    await db.VendorCategories
                        .AnyAsync(x =>
                            x.VendorCategoryId ==
                            createVendorDto.VendorCategoryId &&
                            x.IsActive == (byte)IsActive.Active);

                if (!vendorCategoryExists)
                {
                    return ApiResponseHelper.Failure<VendorDto>(
                        "Vendor category not found.",
                        "VENDOR_CATEGORY_NOT_FOUND",
                        $"Active vendor category with ID {createVendorDto.VendorCategoryId} does not exist.");
                }

                bool companyExists = await db.Companies
                    .AnyAsync(x =>
                        x.CompanyId == createVendorDto.CompanyId &&
                        x.IsActive == (byte)IsActive.Active);

                if (!companyExists)
                {
                    return ApiResponseHelper.Failure<VendorDto>(
                        "Company not found.",
                        "COMPANY_NOT_FOUND",
                        $"Active company with ID {createVendorDto.CompanyId} does not exist.");
                }

                bool vendorCodeExists = await db.Vendors
                    .AnyAsync(x =>
                        x.VendorCode == createVendorDto.VendorCode);

                if (vendorCodeExists)
                {
                    return ApiResponseHelper.Failure<VendorDto>(
                        "Vendor code already exists.",
                        "DUPLICATE_VENDOR_CODE",
                        $"Vendor with code {createVendorDto.VendorCode} already exists.");
                }

                bool createdByExists = await db.Users
                    .AnyAsync(x =>
                        x.UserId == createVendorDto.CreatedBy);

                if (!createdByExists)
                {
                    return ApiResponseHelper.Failure<VendorDto>(
                        "Created by user not found.",
                        "CREATED_BY_USER_NOT_FOUND",
                        $"User with ID {createVendorDto.CreatedBy} does not exist.");
                }

                Vendor vendor =
                    mapper.Map<Vendor>(createVendorDto);

                vendor.VendorId = 0;
                vendor.IsActive = (byte)IsActive.Active;
                vendor.CreatedAt = DateTime.UtcNow;
                vendor.ModifiedAt = DateTime.UtcNow;
                vendor.ModifiedBy = createVendorDto.CreatedBy;

                await db.Vendors.AddAsync(vendor);
                await db.SaveChangesAsync();

                vendorCacheVersion++;

                Vendor createdVendor = await GetVendorQuery()
                    .FirstOrDefaultAsync(
                        x => x.VendorId == vendor.VendorId);

                VendorDto result =
                    mapper.Map<VendorDto>(createdVendor);

                ApiResponse<VendorDto> response =
                    ApiResponseHelper.SuccessRes(
                        result,
                        "Vendor created successfully.");

                string cacheKey =
                    $"vendor_{vendor.VendorId}";

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<VendorDto>(
                    "Failed to create vendor.",
                    "VENDOR_CREATE_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<VendorDto>> UpdateVendorAsync(
            int id,
            UpdateVendorDto updateVendorDto)
        {
            try
            {
                Vendor vendor = await GetVendorQuery()
                    .FirstOrDefaultAsync(
                        x => x.VendorId == id);

                if (vendor == null)
                {
                    return ApiResponseHelper.Failure<VendorDto>(
                        "Vendor not found.",
                        "VENDOR_NOT_FOUND",
                        $"Vendor with ID {id} does not exist.");
                }

                bool vendorCategoryExists =
                    await db.VendorCategories
                        .AnyAsync(x =>
                            x.VendorCategoryId ==
                            updateVendorDto.VendorCategoryId &&
                            x.IsActive == (byte)IsActive.Active);

                if (!vendorCategoryExists)
                {
                    return ApiResponseHelper.Failure<VendorDto>(
                        "Vendor category not found.",
                        "VENDOR_CATEGORY_NOT_FOUND",
                        $"Active vendor category with ID {updateVendorDto.VendorCategoryId} does not exist.");
                }

                bool companyExists = await db.Companies
                    .AnyAsync(x =>
                        x.CompanyId == updateVendorDto.CompanyId &&
                        x.IsActive == (byte)IsActive.Active);

                if (!companyExists)
                {
                    return ApiResponseHelper.Failure<VendorDto>(
                        "Company not found.",
                        "COMPANY_NOT_FOUND",
                        $"Active company with ID {updateVendorDto.CompanyId} does not exist.");
                }

                bool vendorCodeExists = await db.Vendors
                    .AnyAsync(x =>
                        x.VendorCode == updateVendorDto.VendorCode &&
                        x.VendorId != id);

                if (vendorCodeExists)
                {
                    return ApiResponseHelper.Failure<VendorDto>(
                        "Vendor code already exists.",
                        "DUPLICATE_VENDOR_CODE",
                        $"Vendor with code {updateVendorDto.VendorCode} already exists.");
                }

                bool modifiedByExists = await db.Users
                    .AnyAsync(x =>
                        x.UserId == updateVendorDto.ModifiedBy);

                if (!modifiedByExists)
                {
                    return ApiResponseHelper.Failure<VendorDto>(
                        "Modified by user not found.",
                        "MODIFIED_BY_USER_NOT_FOUND",
                        $"User with ID {updateVendorDto.ModifiedBy} does not exist.");
                }

                vendor.VendorCode =
                    updateVendorDto.VendorCode;

                vendor.VendorCategoryId =
                    updateVendorDto.VendorCategoryId;

                vendor.CompanyId =
                    updateVendorDto.CompanyId;

                vendor.BankAccount =
                    updateVendorDto.BankAccount;

                vendor.PAN =
                    updateVendorDto.PAN;

                vendor.PerformanceScore =
                    updateVendorDto.PerformanceScore;

                vendor.IsVerified =
                    updateVendorDto.IsVerified;

                vendor.ModifiedBy =
                    updateVendorDto.ModifiedBy;

                vendor.ModifiedAt =
                    DateTime.UtcNow;

                await db.SaveChangesAsync();

                vendorCacheVersion++;

                Vendor updatedVendor = await GetVendorQuery()
                    .FirstOrDefaultAsync(
                        x => x.VendorId == id);

                VendorDto result =
                    mapper.Map<VendorDto>(updatedVendor);

                ApiResponse<VendorDto> response =
                    ApiResponseHelper.SuccessRes(
                        result,
                        "Vendor updated successfully.");

                string cacheKey = $"vendor_{id}";

                cache.Remove(cacheKey);

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<VendorDto>(
                    "Failed to update vendor.",
                    "VENDOR_UPDATE_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<string>> DeleteVendorAsync(
            int id)
        {
            try
            {
                Vendor vendor = await db.Vendors
                    .FirstOrDefaultAsync(
                        x => x.VendorId == id);

                if (vendor == null)
                {
                    return ApiResponseHelper.Failure<string>(
                        "Vendor not found.",
                        "VENDOR_NOT_FOUND",
                        $"Vendor with ID {id} does not exist.");
                }

                if (vendor.IsActive ==
                    (byte)IsActive.Inactive)
                {
                    return ApiResponseHelper.Failure<string>(
                        "Vendor already deleted.",
                        "409",
                        "Vendor is already inactive.");
                }

                vendor.IsActive =
                    (byte)IsActive.Inactive;

                vendor.ModifiedAt =
                    DateTime.UtcNow;

                await db.SaveChangesAsync();

                vendorCacheVersion++;

                string cacheKey =
                    $"vendor_{id}";

                cache.Remove(cacheKey);

                return ApiResponseHelper.SuccessRes(
                    "Deleted",
                    "Vendor deleted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<string>(
                    "Failed to delete vendor.",
                    "VENDOR_DELETE_ERROR",
                    ex.Message);
            }
        }
    }
}