using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Fincore.Domain.Models;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Fincore.Domain.Enums;
namespace Fincore.Infrastructure.Services.MasterTable
{
    public class UserService : IUserService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IMemoryCache cache;
        private static int userCacheVersion = 1;

        public UserService(
            AppDbContext db,
            IMapper mapper,
            IPasswordHasher<User> passwordHasher,
            IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.passwordHasher = passwordHasher;
            this.cache = cache;
        }

        public async Task<ApiResponse<List<UserDto>>> GetAllUsersAsync(
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
                {
                    search = search.Trim();
                }

                string cacheKey =
                    $"users_{userCacheVersion}_page_{pageNumber}_size_{pageSize}_search_{search}";

                if (cache.TryGetValue(
                    cacheKey,
                    out ApiResponse<List<UserDto>> cachedData))
                {
                    Console.WriteLine("GET ALL USERS - Data returned from CACHE");

                    return cachedData;
                }

                Console.WriteLine("GET ALL USERS - Data returned from DATABASE");

                //IQueryable<User> query = db.Users
                //    .Include(x => x.Role);

                IQueryable<User> query = db.Users
                    .Where(x => x.IsActive == (byte)IsActive.Active)
                    .Include(x => x.Role);

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        x.FullName.Contains(search) ||
                        x.Email.Contains(search));
                }

                int totalRecords = await query.CountAsync();

                List<User> users = await query
                    .OrderBy(x => x.UserId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                List<UserDto> userDtos =
                    mapper.Map<List<UserDto>>(users);

                var metadata = new
                {
                    pageNumber,
                    pageSize,
                    totalPages = (int)Math.Ceiling(
                        totalRecords / (double)pageSize)
                };

                ApiResponse<List<UserDto>> response =
                    ApiResponseHelper.SuccessRes(
                        userDtos,
                        "Users retrieved successfully.",
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
                return ApiResponseHelper.Failure<List<UserDto>>(
                    "Failed to retrieve users.",
                    "USER_GET_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(int id)
        {
            try
            {
                string cacheKey = $"user_{id}";

                if (cache.TryGetValue(
                    cacheKey,
                    out ApiResponse<UserDto> cachedData))
                {
                    Console.WriteLine("GET USER BY ID - Data returned from CACHE");

                    return cachedData;
                }

                Console.WriteLine("GET USER BY ID - Data returned from DATABASE");

                //var user = await db.Users
                //    .Include(x => x.Role)
                //    .FirstOrDefaultAsync(x => x.UserId == id);

                var user = await db.Users
                        .Include(x => x.Role)
                        .FirstOrDefaultAsync(x =>x.UserId == id && x.IsActive == (byte)IsActive.Active);

                if (user == null)
                {
                    return ApiResponseHelper.Failure<UserDto>(
                        "User not found.",
                        "USER_NOT_FOUND",
                        $"User with ID {id} does not exist.");
                }

                var userDto = mapper.Map<UserDto>(user);

                ApiResponse<UserDto> response =
                    ApiResponseHelper.SuccessRes(
                        userDto,
                        "User retrieved successfully.");

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<UserDto>(
                    "Failed to retrieve user.",
                    "USER_GET_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<UserDto>> CreateUserAsync(
            CreateUserDto createUserDto)
        {
            try
            {
                var roleExists = await db.Roles
                    .AnyAsync(x => x.RoleId == createUserDto.RoleId);

                if (!roleExists)
                {
                    return ApiResponseHelper.Failure<UserDto>(
                        "Role not found.",
                        "ROLE_NOT_FOUND",
                        $"Role with ID {createUserDto.RoleId} does not exist.");
                }

                var emailExists = await db.Users
                    .AnyAsync(x => x.Email == createUserDto.Email);

                if (emailExists)
                {
                    return ApiResponseHelper.Failure<UserDto>(
                        "Email already exists.",
                        "DUPLICATE_EMAIL",
                        $"User with email {createUserDto.Email} already exists.");
                }

                var createdByExists = await db.Users
                    .AnyAsync(x => x.UserId == createUserDto.CreatedBy);

                if (!createdByExists)
                {
                    return ApiResponseHelper.Failure<UserDto>(
                        "Created by user not found.",
                        "CREATED_BY_USER_NOT_FOUND",
                        $"User with ID {createUserDto.CreatedBy} does not exist.");
                }

                var user = mapper.Map<User>(createUserDto);

                user.UserId = 0;

                user.PasswordHash = passwordHasher.HashPassword(
                    user,
                    createUserDto.Password);

                user.CreatedAt = DateTime.UtcNow;
                user.ModifiedAt = DateTime.UtcNow;
                user.ModifiedBy = createUserDto.CreatedBy;

                user.LastLogin = null;
                user.RefreshToken = string.Empty;

                await db.Users.AddAsync(user);
                await db.SaveChangesAsync();

                userCacheVersion++;

                var createdUser = await db.Users
                    .Include(x => x.Role)
                    .FirstOrDefaultAsync(x => x.UserId == user.UserId);

                var result = mapper.Map<UserDto>(createdUser);

                ApiResponse<UserDto> response =
                    ApiResponseHelper.SuccessRes(
                        result,
                        "User created successfully.");

                string cacheKey = $"user_{user.UserId}";

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<UserDto>(
                    "Failed to create user.",
                    "USER_CREATE_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<UserDto>> UpdateUserAsync(
            int id,
            UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await db.Users
                    .FirstOrDefaultAsync(x => x.UserId == id);

                if (user == null)
                {
                    return ApiResponseHelper.Failure<UserDto>(
                        "User not found.",
                        "USER_NOT_FOUND",
                        $"User with ID {id} does not exist.");
                }

                var roleExists = await db.Roles
                    .AnyAsync(x => x.RoleId == updateUserDto.RoleId);

                if (!roleExists)
                {
                    return ApiResponseHelper.Failure<UserDto>(
                        "Role not found.",
                        "ROLE_NOT_FOUND",
                        $"Role with ID {updateUserDto.RoleId} does not exist.");
                }

                var emailExists = await db.Users
                    .AnyAsync(x =>
                        x.Email == updateUserDto.Email &&
                        x.UserId != id);

                if (emailExists)
                {
                    return ApiResponseHelper.Failure<UserDto>(
                        "Email already exists.",
                        "DUPLICATE_EMAIL",
                        $"User with email {updateUserDto.Email} already exists.");
                }

                var modifiedByExists = await db.Users
                    .AnyAsync(x =>
                        x.UserId == updateUserDto.ModifiedBy);

                if (!modifiedByExists)
                {
                    return ApiResponseHelper.Failure<UserDto>(
                        "Modified by user not found.",
                        "MODIFIED_BY_USER_NOT_FOUND",
                        $"User with ID {updateUserDto.ModifiedBy} does not exist.");
                }

                user.RoleId = updateUserDto.RoleId;
                user.FullName = updateUserDto.FullName;
                user.Email = updateUserDto.Email;
                user.UserCategory = updateUserDto.UserCategory;
                user.Phone = updateUserDto.Phone;
                user.IsActive = updateUserDto.IsActive;

                user.ModifiedBy = updateUserDto.ModifiedBy;
                user.ModifiedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                userCacheVersion++;

                var updatedUser = await db.Users
                    .Include(x => x.Role)
                    .FirstOrDefaultAsync(x => x.UserId == id);

                var result = mapper.Map<UserDto>(updatedUser);

                ApiResponse<UserDto> response =
                    ApiResponseHelper.SuccessRes(
                        result,
                        "User updated successfully.");

                string cacheKey = $"user_{id}";

                cache.Remove(cacheKey);

                cache.Set(
                    cacheKey,
                    response,
                    TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<UserDto>(
                    "Failed to update user.",
                    "USER_UPDATE_ERROR",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(int id)
        {
            try
            {
                var user = await db.Users
                    .FirstOrDefaultAsync(x => x.UserId == id);

                if (user == null)
                {
                    return ApiResponseHelper.Failure<bool>(
                        "User not found.",
                        "USER_NOT_FOUND",
                        $"User with ID {id} does not exist.");
                }

                if (user.IsActive == (byte)IsActive.Inactive)
                {
                    return ApiResponseHelper.Failure<bool>(
                        "User already deleted.",
                        "USER_ALREADY_DELETED",
                        $"User with ID {id} is already inactive.");
                }

                user.IsActive = (byte)IsActive.Inactive;
                user.ModifiedAt = DateTime.UtcNow;
                user.ModifiedBy = 1;

                await db.SaveChangesAsync();

                userCacheVersion++;

                string cacheKey = $"user_{id}";

                cache.Remove(cacheKey);

                return ApiResponseHelper.SuccessRes(
                    true,
                    "User deleted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponseHelper.Failure<bool>(
                    "Failed to delete user.",
                    "USER_DELETE_ERROR",
                    ex.Message);
            }
        }
    }
}