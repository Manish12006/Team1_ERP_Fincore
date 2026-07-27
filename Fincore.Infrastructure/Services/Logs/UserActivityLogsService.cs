using Fincore.Application.CommonHelper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Logs;
using Fincore.Application.DTO.MasterTable;
using Fincore.Application.Interfaces.Logs;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.Logs
{
    public class UserActivityLogService : IUserActivityLogService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public UserActivityLogService(
            AppDbContext context,
            IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<UserActivityLogResponseDto>>> GetAllAsync(
            int page,
            int pageSize,
            string? activityType = null)
        {
            string cacheKey =
                $"UserActivityLogs_{page}_{pageSize}_{activityType}";

            if (_cache.TryGetValue(
                cacheKey,
                out ApiResponse<PagedResponse<UserActivityLogResponseDto>>? cached))
            {
                return cached!;
            }

            var query = _context.UserActivityLogs
                .Include(x => x.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(activityType))
            {
                query = query.Where(x =>
                    x.ActivityType.Contains(activityType));
            }

            int totalRecords = await query.CountAsync();

            if (page < 1)
            {
                return ApiResponseHelper.Failure<PagedResponse<UserActivityLogResponseDto>>(
                    "Invalid page number.", "INVALID_PAGE", "Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<PagedResponse<UserActivityLogResponseDto>>(
                "Invalid page number.", "INVALID_PAGE", "Page number must be greater than or equal to 1.");
            }

            var data = await query
                .OrderByDescending(x => x.ActivityDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new UserActivityLogResponseDto
                {
                    UserActivityLogId = x.UserActivityLogId,
                    UserId = x.UserId,
                    UserName = x.User.FullName,
                    ActivityType = x.ActivityType,
                    Module = x.Module,
                    ActivityDate = x.ActivityDate
                })
                .ToListAsync();

            var response = new PagedResponse<UserActivityLogResponseDto>
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
                "User activity logs fetched successfully",
                totalRecords);

            _cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(10));

            return result;
        }

        public async Task<ApiResponse<UserActivityLogResponseDto>> GetByIdAsync(
            long id)
        {
            string cacheKey =
                $"UserActivityLog_{id}";

            if (_cache.TryGetValue(
                cacheKey,
                out ApiResponse<UserActivityLogResponseDto>? cached))
            {
                return cached!;
            }

            var data = await _context.UserActivityLogs
                .Include(x => x.User)
                .Where(x => x.UserActivityLogId == id)
                .Select(x => new UserActivityLogResponseDto
                {
                    UserActivityLogId = x.UserActivityLogId,
                    UserId = x.UserId,
                    UserName = x.User.FullName,
                    ActivityType = x.ActivityType,
                    Module = x.Module,
                    ActivityDate = x.ActivityDate
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return ApiResponseHelper.Failure<UserActivityLogResponseDto>(
                    "User activity log not found",
                    "NOT_FOUND",
                    "Invalid UserActivityLog Id");
            }

            var result = ApiResponseHelper.SuccessRes(
                data,
                "User activity log fetched successfully");

            _cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(10));

            return result;
        }
        public async Task<ApiResponse<UserActivityLogResponseDto>> CreateAsync(
    UserActivityLogDto dto)
        {
            bool userExists = await _context.Users
                .AnyAsync(x => x.UserId == dto.UserId);

            if (!userExists)
            {
                return ApiResponseHelper.Failure<UserActivityLogResponseDto>(
                    "User not found",
                    "NOT_FOUND",
                    "Invalid User Id");
            }

            var activityLog = new Domain.Models.UserActivityLog
            {
                UserId = dto.UserId,
                ActivityType = dto.ActivityType,
                Module = dto.Module,
                ActivityDate = dto.ActivityDate
            };

            await _context.UserActivityLogs.AddAsync(activityLog);

            await _context.SaveChangesAsync();

            RemoveCache();

            return await GetByIdAsync(activityLog.UserActivityLogId);
        }


        public async Task<ApiResponse<UserActivityLogResponseDto>> UpdateAsync(
            long id,
            UserActivityLogDto dto)
        {
            var activityLog = await _context.UserActivityLogs
                .FirstOrDefaultAsync(x =>
                    x.UserActivityLogId == id);

            if (activityLog == null)
            {
                return ApiResponseHelper.Failure<UserActivityLogResponseDto>(
                    "User activity log not found",
                    "NOT_FOUND",
                    "Invalid UserActivityLog Id");
            }

            bool userExists = await _context.Users
                .AnyAsync(x => x.UserId == dto.UserId);

            if (!userExists)
            {
                return ApiResponseHelper.Failure<UserActivityLogResponseDto>(
                    "User not found",
                    "NOT_FOUND",
                    "Invalid User Id");
            }

            activityLog.UserId = dto.UserId;
            activityLog.ActivityType = dto.ActivityType;
            activityLog.Module = dto.Module;
            activityLog.ActivityDate = dto.ActivityDate;

            await _context.SaveChangesAsync();

            _cache.Remove($"UserActivityLog_{id}");
            RemoveCache();

            return await GetByIdAsync(id);
        }


        public async Task<ApiResponse<bool>> DeleteAsync(
            long id)
        {
            var activityLog = await _context.UserActivityLogs
                .FirstOrDefaultAsync(x =>
                    x.UserActivityLogId == id);

            if (activityLog == null)
            {
                return ApiResponseHelper.Failure<bool>(
                    "User activity log not found",
                    "NOT_FOUND",
                    "Invalid UserActivityLog Id");
            }

            _context.UserActivityLogs.Remove(activityLog);

            await _context.SaveChangesAsync();

            _cache.Remove($"UserActivityLog_{id}");
            RemoveCache();

            return ApiResponseHelper.SuccessRes(
                true,
                "User activity log deleted successfully");
        }


        public async Task<List<object>> GetUsersDropdownAsync()
        {
            return await _context.Users
                .Select(x => new
                {
                    id = x.UserId,
                    name = x.FullName
                })
                .ToListAsync<object>();
        }
        private void RemoveCache()
        {
            _cache.Remove("UserActivityLogs");
        }
    }
}