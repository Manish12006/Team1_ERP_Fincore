
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
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public AuditLogService(
            AppDbContext context,
            IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }


        public async Task<ApiResponse<PagedResponse<AuditLogResponseDto>>> GetAllAsync(
            int page,
            int pageSize,
            string? entityName = null)
        {
            string cacheKey =
                $"AuditLogs_{page}_{pageSize}_{entityName}";


            if (_cache.TryGetValue(
                cacheKey,
                out ApiResponse<PagedResponse<AuditLogResponseDto>>? cached))
            {
                return cached!;
            }


            var query = _context.AuditLogs
                .Include(x => x.AuditByUser)
                .AsQueryable();


            if (!string.IsNullOrEmpty(entityName))
            {
                query = query.Where(x =>
                    x.EntityName.Contains(entityName));
            }


            int totalRecords = await query.CountAsync();

            if (page < 1)
            {
                return ApiResponseHelper.Failure<PagedResponse<AuditLogResponseDto>>(
                    "Invalid page number.", "INVALID_PAGE", "Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<PagedResponse<AuditLogResponseDto>>(
                "Invalid page number.", "INVALID_PAGE", "Page number must be greater than or equal to 1.");
            }

            if (page < 1)
            {
                return ApiResponseHelper.Failure<PagedResponse<AuditLogResponseDto>>(
                    "Invalid page number.", "INVALID_PAGE", "Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<PagedResponse<AuditLogResponseDto>>(
                "Invalid page number.", "INVALID_PAGE", "Page number must be greater than or equal to 1.");
            }
            var data = await query
                .OrderByDescending(x => x.AuditAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AuditLogResponseDto
                {
                    AuditLogId = x.AuditLogId,
                    EntityName = x.EntityName,
                    EntityId = x.EntityId,
                    OperationType = x.OperationType,
                    OldData = x.OldData,
                    NewData = x.NewData,
                    AuditBy = x.AuditBy,
                    AuditByName = x.AuditByUser != null
              ? x.AuditByUser.FullName
              : null,
                    AuditAt = x.AuditAt
                })
                .ToListAsync();


            var response = new PagedResponse<AuditLogResponseDto>
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
                "Audit logs fetched successfully",
                totalRecords);


            _cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(10));


            return result;
        }


        public async Task<ApiResponse<AuditLogResponseDto>> GetByIdAsync(
            long id)
        {
            string cacheKey = $"AuditLog_{id}";


            if (_cache.TryGetValue(
                cacheKey,
                out ApiResponse<AuditLogResponseDto>? cached))
            {
                return cached!;
            }


            var auditLog = await _context.AuditLogs
                .Include(x => x.AuditByUser)
                .Where(x => x.AuditLogId == id)
                .Select(x => new AuditLogResponseDto
                {
                    AuditLogId = x.AuditLogId,
                    EntityName = x.EntityName,
                    EntityId = x.EntityId,
                    OperationType = x.OperationType,
                    OldData = x.OldData,
                    NewData = x.NewData,
                    AuditBy = x.AuditBy,
                    AuditByName = x.AuditByUser != null
                    ? x.AuditByUser.FullName
                    : null,
                    AuditAt = x.AuditAt
                })
                .FirstOrDefaultAsync();


            if (auditLog == null)
            {
                return ApiResponseHelper.Failure<AuditLogResponseDto>(
                    "Audit log not found",
                    "NOT_FOUND",
                    "Invalid AuditLog Id");
            }


            var result = ApiResponseHelper.SuccessRes(
                auditLog,
                "Audit log fetched successfully");


            _cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(10));


            return result;
        }
        public async Task<ApiResponse<AuditLogResponseDto>> CreateAsync(
    AuditLogDto dto)
        {
            bool userExists = await _context.Users
                .AnyAsync(x => x.UserId == dto.AuditBy);

            if (!userExists)
            {
                return ApiResponseHelper.Failure<AuditLogResponseDto>(
                    "User not found",
                    "NOT_FOUND",
                    "Invalid AuditBy user id");
            }


            var auditLog = new Domain.Models.AuditLog
            {
                EntityName = dto.EntityName,
                EntityId = dto.EntityId,
                OperationType = dto.OperationType,
                OldData = dto.OldData,
                NewData = dto.NewData,
                AuditBy = dto.AuditBy,
                AuditAt = dto.AuditAt
            };


            await _context.AuditLogs.AddAsync(auditLog);

            await _context.SaveChangesAsync();


            RemoveCache();


            return await GetByIdAsync(auditLog.AuditLogId);
        }



        public async Task<ApiResponse<AuditLogResponseDto>> UpdateAsync(
            long id,
            AuditLogDto dto)
        {
            var auditLog = await _context.AuditLogs
                .FirstOrDefaultAsync(x => x.AuditLogId == id);


            if (auditLog == null)
            {
                return ApiResponseHelper.Failure<AuditLogResponseDto>(
                    "Audit log not found",
                    "NOT_FOUND",
                    "Invalid AuditLog Id");
            }


            bool userExists = await _context.Users
                .AnyAsync(x => x.UserId == dto.AuditBy);


            if (!userExists)
            {
                return ApiResponseHelper.Failure<AuditLogResponseDto>(
                    "User not found",
                    "NOT_FOUND",
                    "Invalid AuditBy user id");
            }


            auditLog.EntityName = dto.EntityName;
            auditLog.EntityId = dto.EntityId;
            auditLog.OperationType = dto.OperationType;
            auditLog.OldData = dto.OldData;
            auditLog.NewData = dto.NewData;
            auditLog.AuditBy = dto.AuditBy;
            auditLog.AuditAt = dto.AuditAt;


            await _context.SaveChangesAsync();


            _cache.Remove($"AuditLog_{id}");
            RemoveCache();


            return await GetByIdAsync(id);
        }



        public async Task<ApiResponse<bool>> DeleteAsync(
            long id)
        {
            var auditLog = await _context.AuditLogs
                .FirstOrDefaultAsync(x => x.AuditLogId == id);


            if (auditLog == null)
            {
                return ApiResponseHelper.Failure<bool>(
                    "Audit log not found",
                    "NOT_FOUND",
                    "Invalid AuditLog Id");
            }


            _context.AuditLogs.Remove(auditLog);

            await _context.SaveChangesAsync();


            _cache.Remove($"AuditLog_{id}");
            RemoveCache();


            return ApiResponseHelper.SuccessRes(
                true,
                "Audit log deleted successfully");
        }
        private void RemoveCache()
        {
            _cache.Remove("AuditLogs");
        }
    }
}