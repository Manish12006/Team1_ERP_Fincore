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
    public class RoleService : IRoleService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache cache;
        private static int roleCacheVersion = 1;

        public RoleService(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }

        private IQueryable<Role> GetRoleQuery()
        {
            return db.Roles
                .Where(x => x.IsActive == (byte)IsActive.Active);
        }

        public async Task<ApiResponse<List<RoleDto>>> GetAllRolesAsync(
            int pageNumber,
            int pageSize,
            string? search)
        {
            try
            {
                if (pageNumber <= 0)
                    pageNumber = 1;

                if (pageSize <= 0)
                    pageSize = 10;

                if (!string.IsNullOrEmpty(search))
                    search = search.Trim();

                string cacheKey =
                    $"roles_{roleCacheVersion}_page_{pageNumber}_size_{pageSize}_search_{search}";

                if (cache.TryGetValue(
                    cacheKey,
                    out ApiResponse<List<RoleDto>> cachedData))
                {
                    return cachedData;
                }

                IQueryable<Role> query = GetRoleQuery();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        x.RoleName.Contains(search) ||
                        (x.Description != null &&
                         x.Description.Contains(search)));
                }

                int totalRecords = await query.CountAsync();

                List<Role> roles = await query
                    .OrderBy(x => x.RoleId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                List<RoleDto> roleDtos =
                    mapper.Map<List<RoleDto>>(roles);

                var metadata = new
                {
                    pageNumber,
                    pageSize,
                    totalPages = (int)Math.Ceiling(
                        totalRecords / (double)pageSize)
                };

                ApiResponse<List<RoleDto>> response =
                    ApiResponseHelper.SuccessRes(
                        roleDtos,
                        "Roles retrieved successfully.",
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
                return ApiResponseHelper.Failure<List<RoleDto>>(
                    "Failed to retrieve roles.",
                    "ROLE_GET_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<RoleDto>> GetRoleByIdAsync(int id)
        {
            try
            {
                string cacheKey = $"role_{id}";

                if (cache.TryGetValue(
                    cacheKey,
                    out ApiResponse<RoleDto> cachedData))
                {
                    return cachedData;
                }

                Role role = await GetRoleQuery()
                    .FirstOrDefaultAsync(x => x.RoleId == id);

                if (role == null)
                {
                    return ApiResponseHelper.Failure<RoleDto>(
                        "Role not found.",
                        "ROLE_NOT_FOUND",
                        $"Role with ID {id} does not exist.");
                }

                RoleDto roleDto = mapper.Map<RoleDto>(role);

                ApiResponse<RoleDto> response =
                    ApiResponseHelper.SuccessRes(
                        roleDto,
                        "Role retrieved successfully.");

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<RoleDto>(
                    "Failed to retrieve role.",
                    "ROLE_GET_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<RoleDto>> CreateRoleAsync(
            CreateRoleDto createRoleDto)
        {
            try
            {
                Role existingRole = await db.Roles
                    .FirstOrDefaultAsync(x =>
                        x.RoleName.ToLower() ==
                        createRoleDto.RoleName.ToLower() &&
                        x.IsActive == (byte)IsActive.Active);

                if (existingRole != null)
                {
                    return ApiResponseHelper.Failure<RoleDto>(
                        "Role name already exists.",
                        "DUPLICATE_ROLE_NAME",
                        $"Role with name {createRoleDto.RoleName} already exists.");
                }

                bool createdByExists = await db.Users
                    .AnyAsync(x =>
                        x.UserId == createRoleDto.CreatedBy);

                if (!createdByExists)
                {
                    return ApiResponseHelper.Failure<RoleDto>(
                        "Created by user not found.",
                        "CREATED_BY_USER_NOT_FOUND",
                        $"User with ID {createRoleDto.CreatedBy} does not exist.");
                }

                Role role = mapper.Map<Role>(createRoleDto);

                role.RoleId = 0;
                role.IsActive = (byte)IsActive.Active;
                role.CreatedAt = DateTime.UtcNow;
                role.ModifiedAt = DateTime.UtcNow;
                role.ModifiedBy = createRoleDto.CreatedBy;

                await db.Roles.AddAsync(role);
                await db.SaveChangesAsync();

                roleCacheVersion++;

                RoleDto result = mapper.Map<RoleDto>(role);

                ApiResponse<RoleDto> response =
                    ApiResponseHelper.SuccessRes(
                        result,
                        "Role created successfully.");

                string cacheKey = $"role_{role.RoleId}";

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<RoleDto>(
                    "Failed to create role.",
                    "ROLE_CREATE_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<RoleDto>> UpdateRoleAsync(
            int id,
            UpdateRoleDto updateRoleDto)
        {
            try
            {
                Role role = await GetRoleQuery()
                    .FirstOrDefaultAsync(x => x.RoleId == id);

                if (role == null)
                {
                    return ApiResponseHelper.Failure<RoleDto>(
                        "Role not found.",
                        "ROLE_NOT_FOUND",
                        $"Role with ID {id} does not exist.");
                }

                Role existingRole = await db.Roles
                    .FirstOrDefaultAsync(x =>
                        x.RoleName.ToLower() ==
                        updateRoleDto.RoleName.ToLower() &&
                        x.RoleId != id &&
                        x.IsActive == (byte)IsActive.Active);

                if (existingRole != null)
                {
                    return ApiResponseHelper.Failure<RoleDto>(
                        "Role name already exists.",
                        "DUPLICATE_ROLE_NAME",
                        $"Role with name {updateRoleDto.RoleName} already exists.");
                }

                bool modifiedByExists = await db.Users
                    .AnyAsync(x =>
                        x.UserId == updateRoleDto.ModifiedBy);

                if (!modifiedByExists)
                {
                    return ApiResponseHelper.Failure<RoleDto>(
                        "Modified by user not found.",
                        "MODIFIED_BY_USER_NOT_FOUND",
                        $"User with ID {updateRoleDto.ModifiedBy} does not exist.");
                }

                role.RoleName = updateRoleDto.RoleName;
                role.Description = updateRoleDto.Description;
                role.ModifiedBy = updateRoleDto.ModifiedBy;
                role.ModifiedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                roleCacheVersion++;

                RoleDto result = mapper.Map<RoleDto>(role);

                ApiResponse<RoleDto> response =
                    ApiResponseHelper.SuccessRes(
                        result,
                        "Role updated successfully.");

                string cacheKey = $"role_{id}";

                cache.Remove(cacheKey);

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<RoleDto>(
                    "Failed to update role.",
                    "ROLE_UPDATE_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> DeleteRoleAsync(int id)
        {
            try
            {
                Role role = await db.Roles
                    .FirstOrDefaultAsync(x => x.RoleId == id);

                if (role == null)
                {
                    return ApiResponseHelper.Failure<bool>(
                        "Role not found.",
                        "ROLE_NOT_FOUND",
                        $"Role with ID {id} does not exist.");
                }

                if (role.IsActive == (byte)IsActive.Inactive)
                {
                    return ApiResponseHelper.Failure<bool>(
                        "Role already deleted.",
                        "ROLE_ALREADY_DELETED",
                        "Role is already inactive.");
                }

                role.IsActive = (byte)IsActive.Inactive;
                role.ModifiedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                roleCacheVersion++;

                string cacheKey = $"role_{id}";

                cache.Remove(cacheKey);

                return ApiResponseHelper.SuccessRes(
                    true,
                    "Role deleted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<bool>(
                    "Failed to delete role.",
                    "ROLE_DELETE_ERROR",
                    ex.Message);
            }
        }
    }
}