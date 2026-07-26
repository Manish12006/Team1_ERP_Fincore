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
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache cache;
        private static int permissionCacheVersion = 1;

        public PermissionService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }

        private IQueryable<Permission> GetPermissionQuery()
        {
            return db.Permissions
                .Where(x => x.IsActive == (byte)IsActive.Active)
                .Include(x => x.Role)
                .Include(x => x.MasterType);
        }

        public async Task<ApiResponse<List<PermissionDto>>> GetAllPermissionsAsync(
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
                    $"permissions_{permissionCacheVersion}_page_{pageNumber}_size_{pageSize}_search_{search}";

                if (cache.TryGetValue(
                    cacheKey,
                    out ApiResponse<List<PermissionDto>> cachedData))
                {
                    Console.WriteLine(
                        "GET ALL PERMISSIONS - Data returned from CACHE");

                    return cachedData;
                }

                Console.WriteLine(
                    "GET ALL PERMISSIONS - Data returned from DATABASE");

                IQueryable<Permission> query = GetPermissionQuery();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        x.PermissionName.Contains(search) ||
                        x.Role.RoleName.Contains(search) ||
                        (x.MasterType != null &&
                         x.MasterType.MasterTypeName.Contains(search)));
                }

                int totalRecords = await query.CountAsync();

                List<Permission> permissions = await query
                    .OrderBy(x => x.PermissionId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                List<PermissionDto> permissionDtos =
                    mapper.Map<List<PermissionDto>>(permissions);

                var metadata = new
                {
                    pageNumber,
                    pageSize,
                    totalPages = (int)Math.Ceiling(
                        (double)totalRecords / pageSize)
                };

                ApiResponse<List<PermissionDto>> response =
                    ApiResponseHelper.SuccessRes(
                        permissionDtos,
                        "Permissions retrieved successfully.",
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
                return ApiResponseHelper.Failure<List<PermissionDto>>(
                    "Failed to retrieve permissions.",
                    "PERMISSION_GET_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<PermissionDto>> GetPermissionByIdAsync(
            int id)
        {
            try
            {
                string cacheKey = $"permission_{id}";

                if (cache.TryGetValue(
                    cacheKey,
                    out ApiResponse<PermissionDto> cachedData))
                {
                    Console.WriteLine(
                        "GET PERMISSION BY ID - Data returned from CACHE");

                    return cachedData;
                }

                Console.WriteLine(
                    "GET PERMISSION BY ID - Data returned from DATABASE");

                Permission permission = await GetPermissionQuery()
                    .FirstOrDefaultAsync(x => x.PermissionId == id);

                if (permission == null)
                {
                    return ApiResponseHelper.Failure<PermissionDto>(
                        "Permission not found.",
                        "PERMISSION_NOT_FOUND",
                        $"Permission with ID {id} does not exist.");
                }

                PermissionDto permissionDto =
                    mapper.Map<PermissionDto>(permission);

                ApiResponse<PermissionDto> response =
                    ApiResponseHelper.SuccessRes(
                        permissionDto,
                        "Permission retrieved successfully.");

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<PermissionDto>(
                    "Failed to retrieve permission.",
                    "PERMISSION_GET_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<PermissionDto>> CreatePermissionAsync(
            CreatePermissionDto createPermissionDto)
        {
            try
            {
                bool roleExists = await db.Roles
                    .AnyAsync(x =>
                        x.RoleId == createPermissionDto.RoleId);

                if (!roleExists)
                {
                    return ApiResponseHelper.Failure<PermissionDto>(
                        "Role not found.",
                        "ROLE_NOT_FOUND",
                        $"Role with ID {createPermissionDto.RoleId} does not exist.");
                }

                if (createPermissionDto.MasterTypeId.HasValue)
                {
                    bool masterTypeExists = await db.MasterTypes
                        .AnyAsync(x =>
                            x.MasterTypeId ==
                            createPermissionDto.MasterTypeId.Value);

                    if (!masterTypeExists)
                    {
                        return ApiResponseHelper.Failure<PermissionDto>(
                            "Master type not found.",
                            "MASTER_TYPE_NOT_FOUND",
                            $"Master type with ID {createPermissionDto.MasterTypeId.Value} does not exist.");
                    }
                }

                bool createdByExists = await db.Users
                    .AnyAsync(x =>
                        x.UserId == createPermissionDto.CreatedBy);

                if (!createdByExists)
                {
                    return ApiResponseHelper.Failure<PermissionDto>(
                        "Created by user not found.",
                        "CREATED_BY_USER_NOT_FOUND",
                        $"User with ID {createPermissionDto.CreatedBy} does not exist.");
                }

                Permission existingPermission = await db.Permissions
                    .FirstOrDefaultAsync(x =>
                        x.PermissionName.ToLower() ==
                        createPermissionDto.PermissionName.ToLower() &&
                        x.RoleId == createPermissionDto.RoleId);

                if (existingPermission != null)
                {
                    return ApiResponseHelper.Failure<PermissionDto>(
                        "Permission already exists.",
                        "DUPLICATE_PERMISSION",
                        "Permission name already exists for this role.");
                }

                Permission permission =
                    mapper.Map<Permission>(createPermissionDto);

                permission.PermissionId = 0;
                permission.IsActive = (byte)IsActive.Active;
                permission.CreatedAt = DateTime.Now;
                permission.ModifiedAt = DateTime.Now;
                permission.ModifiedBy = createPermissionDto.CreatedBy;

                await db.Permissions.AddAsync(permission);
                await db.SaveChangesAsync();

                permissionCacheVersion++;

                Permission createdPermission = await GetPermissionQuery()
                    .FirstOrDefaultAsync(
                        x => x.PermissionId == permission.PermissionId);

                PermissionDto result =
                    mapper.Map<PermissionDto>(createdPermission);

                ApiResponse<PermissionDto> response =
                    ApiResponseHelper.SuccessRes(
                        result,
                        "Permission created successfully.");

                string cacheKey =
                    $"permission_{permission.PermissionId}";

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<PermissionDto>(
                    "Failed to create permission.",
                    "PERMISSION_CREATE_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<PermissionDto>> UpdatePermissionAsync(
            int id,
            UpdatePermissionDto updatePermissionDto)
        {
            try
            {
                Permission permission = await GetPermissionQuery()
                    .FirstOrDefaultAsync(
                        x => x.PermissionId == id);

                if (permission == null)
                {
                    return ApiResponseHelper.Failure<PermissionDto>(
                        "Permission not found.",
                        "PERMISSION_NOT_FOUND",
                        $"Permission with ID {id} does not exist.");
                }

                bool roleExists = await db.Roles
                    .AnyAsync(x =>
                        x.RoleId == updatePermissionDto.RoleId);

                if (!roleExists)
                {
                    return ApiResponseHelper.Failure<PermissionDto>(
                        "Role not found.",
                        "ROLE_NOT_FOUND",
                        $"Role with ID {updatePermissionDto.RoleId} does not exist.");
                }

                if (updatePermissionDto.MasterTypeId.HasValue)
                {
                    bool masterTypeExists = await db.MasterTypes
                        .AnyAsync(x =>
                            x.MasterTypeId ==
                            updatePermissionDto.MasterTypeId.Value);

                    if (!masterTypeExists)
                    {
                        return ApiResponseHelper.Failure<PermissionDto>(
                            "Master type not found.",
                            "MASTER_TYPE_NOT_FOUND",
                            $"Master type with ID {updatePermissionDto.MasterTypeId.Value} does not exist.");
                    }
                }

                bool modifiedByExists = await db.Users
                    .AnyAsync(x =>
                        x.UserId == updatePermissionDto.ModifiedBy);

                if (!modifiedByExists)
                {
                    return ApiResponseHelper.Failure<PermissionDto>(
                        "Modified by user not found.",
                        "MODIFIED_BY_USER_NOT_FOUND",
                        $"User with ID {updatePermissionDto.ModifiedBy} does not exist.");
                }

                Permission existingPermission = await db.Permissions
                    .FirstOrDefaultAsync(x =>
                        x.PermissionName.ToLower() ==
                        updatePermissionDto.PermissionName.ToLower() &&
                        x.RoleId == updatePermissionDto.RoleId &&
                        x.PermissionId != id);

                if (existingPermission != null)
                {
                    return ApiResponseHelper.Failure<PermissionDto>(
                        "Permission already exists.",
                        "DUPLICATE_PERMISSION",
                        "Permission name already exists for this role.");
                }

                permission.PermissionName =
                    updatePermissionDto.PermissionName;

                permission.RoleId =
                    updatePermissionDto.RoleId;

                permission.MasterTypeId =
                    updatePermissionDto.MasterTypeId;

                permission.IsActive =
                    updatePermissionDto.IsActive;

                permission.ModifiedBy =
                    updatePermissionDto.ModifiedBy;

                permission.ModifiedAt =
                    DateTime.Now;

                await db.SaveChangesAsync();

                permissionCacheVersion++;

                Permission updatedPermission = await GetPermissionQuery()
                    .FirstOrDefaultAsync(
                        x => x.PermissionId == id);

                PermissionDto result =
                    mapper.Map<PermissionDto>(updatedPermission);

                ApiResponse<PermissionDto> response =
                    ApiResponseHelper.SuccessRes(
                        result,
                        "Permission updated successfully.");

                string cacheKey = $"permission_{id}";

                cache.Remove(cacheKey);

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<PermissionDto>(
                    "Failed to update permission.",
                    "PERMISSION_UPDATE_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> DeletePermissionAsync(int id)
        {
            try
            {
                Permission permission = await db.Permissions
                    .FirstOrDefaultAsync(
                        x => x.PermissionId == id);

                if (permission == null)
                {
                    return ApiResponseHelper.Failure<bool>(
                        "Permission not found.",
                        "PERMISSION_NOT_FOUND",
                        $"Permission with ID {id} does not exist.");
                }

                if (permission.IsActive ==
                    (byte)IsActive.Inactive)
                {
                    return ApiResponseHelper.Failure<bool>(
                        "Permission already deleted.",
                        "PERMISSION_ALREADY_DELETED",
                        "Permission is already inactive.");
                }

                permission.IsActive =
                    (byte)IsActive.Inactive;

                permission.ModifiedAt =
                    DateTime.Now;

                await db.SaveChangesAsync();

                permissionCacheVersion++;

                string cacheKey = $"permission_{id}";

                cache.Remove(cacheKey);

                return ApiResponseHelper.SuccessRes(
                    true,
                    "Permission deleted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<bool>(
                    "Failed to delete permission.",
                    "PERMISSION_DELETE_ERROR",
                    ex.Message);
            }
        }
    }
}