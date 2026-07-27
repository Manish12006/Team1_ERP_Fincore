
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
    public class NotificationLogService : INotificationLogService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;


        public NotificationLogService(
            AppDbContext context,
            IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }


        public async Task<ApiResponse<PagedResponse<NotificationLogResponseDto>>> GetAllAsync(
            int page,
            int pageSize,
            string? title = null)
        {
            string cacheKey =
                $"NotificationLogs_{page}_{pageSize}_{title}";


            if (_cache.TryGetValue(
                cacheKey,
                out ApiResponse<PagedResponse<NotificationLogResponseDto>>? cached))
            {
                return cached!;
            }


            var query = _context.NotificationLogs
                .Include(x => x.User)
                .AsQueryable();


            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(x =>
                    x.Title.Contains(title));
            }


            int totalRecords = await query.CountAsync();

            if (page < 1)
            {
                return ApiResponseHelper.Failure<PagedResponse<NotificationLogResponseDto>>(
                    "Invalid page number.", "INVALID_PAGE", "Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<PagedResponse<NotificationLogResponseDto>>(
                "Invalid page number.", "INVALID_PAGE", "Page number must be greater than or equal to 1.");
            }


            var data = await query
                .OrderByDescending(x => x.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new NotificationLogResponseDto
                {
                    NotificationLogId = x.NotificationLogId,
                    UserId = x.UserId,
                    UserName = x.User.FullName,
                    Title = x.Title,
                    Message = x.Message,
                    SentAt = x.SentAt
                })
                .ToListAsync();


            var response = new PagedResponse<NotificationLogResponseDto>
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
                "Notification logs fetched successfully",
                totalRecords);


            _cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(10));


            return result;
        }

        public async Task<ApiResponse<NotificationLogResponseDto>> GetByIdAsync(
            long id)
        {
            string cacheKey =
                $"NotificationLog_{id}";


            if (_cache.TryGetValue(
                cacheKey,
                out ApiResponse<NotificationLogResponseDto>? cached))
            {
                return cached!;
            }


            var data = await _context.NotificationLogs
                .Include(x => x.User)
                .Where(x => x.NotificationLogId == id)
                .Select(x => new NotificationLogResponseDto
                {
                    NotificationLogId = x.NotificationLogId,
                    UserId = x.UserId,
                    UserName = x.User.FullName,
                    Title = x.Title,
                    Message = x.Message,
                    SentAt = x.SentAt
                })
                .FirstOrDefaultAsync();


            if (data == null)
            {
                return ApiResponseHelper.Failure<NotificationLogResponseDto>(
                    "Notification log not found",
                    "NOT_FOUND",
                    "Invalid NotificationLog Id");
            }


            var result = ApiResponseHelper.SuccessRes(
                data,
                "Notification log fetched successfully");


            _cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(10));


            return result;
        }

        public async Task<ApiResponse<NotificationLogResponseDto>> CreateAsync(
            NotificationLogDto dto)
        {
            bool userExists = await _context.Users
                .AnyAsync(x => x.UserId == dto.UserId);


            if (!userExists)
            {
                return ApiResponseHelper.Failure<NotificationLogResponseDto>(
                    "User not found",
                    "NOT_FOUND",
                    "Invalid User Id");
            }


            var notification = new Domain.Models.NotificationLog
            {
                UserId = dto.UserId,
                Title = dto.Title,
                Message = dto.Message,
                SentAt = dto.SentAt
            };


            await _context.NotificationLogs.AddAsync(notification);

            await _context.SaveChangesAsync();


            RemoveCache();


            return await GetByIdAsync(notification.NotificationLogId);
        }

        public async Task<ApiResponse<NotificationLogResponseDto>> UpdateAsync(
            long id,
            NotificationLogDto dto)
        {
            var notification = await _context.NotificationLogs
                .FirstOrDefaultAsync(x =>
                    x.NotificationLogId == id);


            if (notification == null)
            {
                return ApiResponseHelper.Failure<NotificationLogResponseDto>(
                    "Notification log not found",
                    "NOT_FOUND",
                    "Invalid NotificationLog Id");
            }


            bool userExists = await _context.Users
                .AnyAsync(x => x.UserId == dto.UserId);


            if (!userExists)
            {
                return ApiResponseHelper.Failure<NotificationLogResponseDto>(
                    "User not found",
                    "NOT_FOUND",
                    "Invalid User Id");
            }


            notification.UserId = dto.UserId;
            notification.Title = dto.Title;
            notification.Message = dto.Message;
            notification.SentAt = dto.SentAt;


            await _context.SaveChangesAsync();


            _cache.Remove($"NotificationLog_{id}");
            RemoveCache();


            return await GetByIdAsync(id);
        }



        public async Task<ApiResponse<bool>> DeleteAsync(
            long id)
        {
            var notification = await _context.NotificationLogs
                .FirstOrDefaultAsync(x =>
                    x.NotificationLogId == id);


            if (notification == null)
            {
                return ApiResponseHelper.Failure<bool>(
                    "Notification log not found",
                    "NOT_FOUND",
                    "Invalid NotificationLog Id");
            }


            _context.NotificationLogs.Remove(notification);

            await _context.SaveChangesAsync();


            _cache.Remove($"NotificationLog_{id}");
            RemoveCache();


            return ApiResponseHelper.SuccessRes(
                true,
                "Notification log deleted successfully");
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
            _cache.Remove("NotificationLogs");
        }
    }
}