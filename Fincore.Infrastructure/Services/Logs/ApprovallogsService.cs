
using Fincore.Application.CommonHelper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Logs;
using Fincore.Application.Interfaces.Logs;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fincore.Infrastructure.Services.Logs
{
    public class ApprovalLogService : IApprovalLogService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public ApprovalLogService(
            AppDbContext context,
            IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<ApprovalLogResponseDto>>> GetAllAsync(
            int page,
            int pageSize,
            string? entityName = null)
        {
            string cacheKey = $"ApprovalLogs_{page}_{pageSize}_{entityName}";

            if (_cache.TryGetValue(
                cacheKey,
                out ApiResponse<PagedResponse<ApprovalLogResponseDto>>? cached))
            {
                return cached!;
            }

            var query = _context.ApprovalLogs
                .Include(x => x.ApproverUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(entityName))
            {
                query = query.Where(x =>
                    x.EntityName.Contains(entityName));
            }

            int totalRecords = await query.CountAsync();

            if (page < 1)
            {
                return ApiResponseHelper.Failure<PagedResponse<ApprovalLogResponseDto>>(
                    "Invalid page number.", "INVALID_PAGE", "Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                return ApiResponseHelper.Failure<PagedResponse<ApprovalLogResponseDto>>(
                "Invalid page number.", "INVALID_PAGE", "Page number must be greater than or equal to 1.");
            }

            var data = await query
                .OrderByDescending(x => x.ActionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ApprovalLogResponseDto
                {
                    ApprovalLogId = x.ApprovalLogId,
                    EntityName = x.EntityName,
                    EntityId = x.EntityId,
                    ApproverId = x.ApproverId,
                    ApproverName = x.ApproverUser.FullName,
                    Status = x.Status,
                    Remarks = x.Remarks,
                    ActionDate = x.ActionDate
                })
                .ToListAsync();

            var response = new PagedResponse<ApprovalLogResponseDto>
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
                "Approval logs fetched successfully",
                totalRecords);

            _cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(10));

            return result;
        }

        public async Task<ApiResponse<ApprovalLogResponseDto>> GetByIdAsync(int id)
        {
            string cacheKey = $"ApprovalLog_{id}";

            if (_cache.TryGetValue(
                cacheKey,
                out ApiResponse<ApprovalLogResponseDto>? cached))
            {
                return cached!;
            }

            var data = await _context.ApprovalLogs
                .Include(x => x.ApproverUser)
                .Where(x => x.ApprovalLogId == id)
                .Select(x => new ApprovalLogResponseDto
                {
                    ApprovalLogId = x.ApprovalLogId,
                    EntityName = x.EntityName,
                    EntityId = x.EntityId,
                    ApproverId = x.ApproverId,
                    ApproverName = x.ApproverUser.FullName,
                    Status = x.Status,
                    Remarks = x.Remarks,
                    ActionDate = x.ActionDate
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return ApiResponseHelper.Failure<ApprovalLogResponseDto>(
                    "Approval log not found",
                    "NOT_FOUND",
                    "Invalid ApprovalLog Id");
            }

            var result = ApiResponseHelper.SuccessRes(
                data,
                "Approval log fetched successfully");

            _cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(10));

            return result;
        }
        public async Task<ApiResponse<ApprovalLogResponseDto>> CreateAsync(
    ApprovalLogDto dto)
        {
            bool userExists = await _context.Users
                .AnyAsync(x => x.UserId == dto.ApproverId);

            if (!userExists)
            {
                return ApiResponseHelper.Failure<ApprovalLogResponseDto>(
                    "User not found",
                    "NOT_FOUND",
                    "Invalid Approver Id");
            }

            var approvalLog = new Domain.Models.ApprovalLog
            {
                EntityName = dto.EntityName,
                EntityId = dto.EntityId,
                ApproverId = dto.ApproverId,
                Status = dto.Status,
                Remarks = dto.Remarks,
                ActionDate = dto.ActionDate
            };

            await _context.ApprovalLogs.AddAsync(approvalLog);

            await _context.SaveChangesAsync();

            RemoveCache();

            return await GetByIdAsync(approvalLog.ApprovalLogId);
        }


        public async Task<ApiResponse<ApprovalLogResponseDto>> UpdateAsync(
            int id,
            ApprovalLogDto dto)
        {
            var approvalLog = await _context.ApprovalLogs
                .FirstOrDefaultAsync(x => x.ApprovalLogId == id);

            if (approvalLog == null)
            {
                return ApiResponseHelper.Failure<ApprovalLogResponseDto>(
                    "Approval log not found",
                    "NOT_FOUND",
                    "Invalid ApprovalLog Id");
            }

            bool userExists = await _context.Users
                .AnyAsync(x => x.UserId == dto.ApproverId);

            if (!userExists)
            {
                return ApiResponseHelper.Failure<ApprovalLogResponseDto>(
                    "User not found",
                    "NOT_FOUND",
                    "Invalid Approver Id");
            }

            approvalLog.EntityName = dto.EntityName;
            approvalLog.EntityId = dto.EntityId;
            approvalLog.ApproverId = dto.ApproverId;
            approvalLog.Status = dto.Status;
            approvalLog.Remarks = dto.Remarks;
            approvalLog.ActionDate = dto.ActionDate;

            await _context.SaveChangesAsync();

            _cache.Remove($"ApprovalLog_{id}");
            RemoveCache();

            return await GetByIdAsync(id);
        }


        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var approvalLog = await _context.ApprovalLogs
                .FirstOrDefaultAsync(x => x.ApprovalLogId == id);

            if (approvalLog == null)
            {
                return ApiResponseHelper.Failure<bool>(
                    "Approval log not found",
                    "NOT_FOUND",
                    "Invalid ApprovalLog Id");
            }

            _context.ApprovalLogs.Remove(approvalLog);

            await _context.SaveChangesAsync();

            _cache.Remove($"ApprovalLog_{id}");
            RemoveCache();

            return ApiResponseHelper.SuccessRes(
                true,
                "Approval log deleted successfully");
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
            _cache.Remove("ApprovalLogs");
        }
    }

}