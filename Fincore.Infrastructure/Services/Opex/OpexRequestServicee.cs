using Microsoft.Extensions.Caching.Memory;
using Fincore.Domain.Enums;
using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTOs.OpexRequest;
using Fincore.Application.Interfaces.Opex;

using Fincore.Domain.Models;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Fincore.Application.DTOs.Common;

namespace Fincore.Infrastructure.Services.Opex
{
    public class OpexRequestService : IOpexRequestService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public OpexRequestService(AppDbContext context, IMapper mapper , IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }

        // Create
        // Create
        public async Task<string> AddOpexRequest(CreateOpexRequestDTO dto)
        {
            // Budget Line Validation
            var budgetLine = await _context.BudgetLines
                .FirstOrDefaultAsync(x => x.BudgetLineId == dto.BudgetLineId);

            if (budgetLine == null)
            {
                return "Budget Line Not Found";
            }

            // User Validation
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == dto.RequestedBy);

            if (user == null)
            {
                return "User Not Found";
            }

            // Title Validation
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return "Title is required";
            }

            if (dto.Title.Length > 30)
            {
                return "Title should not exceed 30 characters";
            }

            // Amount Validation
            if (dto.Amount <= 0)
            {
                return "Amount must be greater than zero";
            }

            // Budget Validation
            decimal utilizedAmount = budgetLine.UtilizedAmount ?? 0;
            decimal remainingBudget = budgetLine.AllocatedAmount - utilizedAmount;

            if (dto.Amount > remainingBudget)
            {
                return "Budget Exceeded";
            }

            // Create Record
            var entity = _mapper.Map<OpexRequest>(dto);

            entity.ApprovalStatus = OpexApprovalStatus.Pending.ToString();
            entity.CreatedAt = DateTime.UtcNow;

            await _context.OpexRequests.AddAsync(entity);

            // Update Utilized Amount
            budgetLine.UtilizedAmount = utilizedAmount + dto.Amount;

            await _context.SaveChangesAsync();

            // Remove Cache
            _cache.Remove("OpexRequestCache");

            return "Success";
        }

        // Get All
        // Get All
        public async Task<List<OpexRequestResponseDTO>> GetOpexRequests(
            string? title,
            int? budgetLineId,
            int? requestedBy,
            string? approvalStatus,
            int page,
            int pageSize)
        {
            string cacheKey =
                $"OpexRequest_{title}_{budgetLineId}_{requestedBy}_{approvalStatus}_{page}_{pageSize}";

            if (!_cache.TryGetValue(cacheKey, out List<OpexRequestResponseDTO> data))
            {
                var query = _context.OpexRequests
                    .Where(x => x.ApprovalStatus != OpexApprovalStatus.Rejected.ToString())
                    .AsQueryable();

                // Title Filter
                if (!string.IsNullOrWhiteSpace(title))
                {
                    query = query.Where(x => x.Title.Contains(title));
                }

                // Budget Line Filter
                if (budgetLineId.HasValue)
                {
                    query = query.Where(x => x.BudgetLineId == budgetLineId.Value);
                }

                // Requested By Filter
                if (requestedBy.HasValue)
                {
                    query = query.Where(x => x.RequestedBy == requestedBy.Value);
                }

                // Approval Status Filter
                if (!string.IsNullOrWhiteSpace(approvalStatus))
                {
                    query = query.Where(x => x.ApprovalStatus == approvalStatus);
                }

                var list = await query
                    .OrderByDescending(x => x.OpexRequestId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                data = _mapper.Map<List<OpexRequestResponseDTO>>(list);

                _cache.Set(cacheKey, data, TimeSpan.FromMinutes(5));
            }

            return data;
        }

        // Get By Id
        // Get By Id
        public async Task<OpexRequestResponseDTO?> GetOpexRequestById(int id)
        {
            var entity = await _context.OpexRequests
                .FirstOrDefaultAsync(x =>
                    x.OpexRequestId == id &&
                    x.ApprovalStatus != OpexApprovalStatus.Rejected.ToString());

            if (entity == null)
            {
                return null;
            }

            return _mapper.Map<OpexRequestResponseDTO>(entity);
        }

        // Update
        // Update
        public async Task UpdateOpexRequest(int id, UpdateOpexRequestDTO dto)
        {
            var entity = await _context.OpexRequests
                .FirstOrDefaultAsync(x =>
                    x.OpexRequestId == id &&
                    x.ApprovalStatus != OpexApprovalStatus.Rejected.ToString());

            if (entity == null)
            {
                return;
            }

            // Title Validation
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return;
            }

            if (dto.Title.Length > 30)
            {
                return;
            }

            // Amount Validation
            if (dto.Amount <= 0)
            {
                return;
            }

            _mapper.Map(dto, entity);

            entity.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _cache.Remove("OpexRequestCache");
        }


        // Soft Delete
        public async Task DeleteOpexRequest(int id)
        {
            var entity = await _context.OpexRequests
                .FirstOrDefaultAsync(x => x.OpexRequestId == id);

            if (entity == null)
            {
                return;
            }

            if (entity.ApprovalStatus == OpexApprovalStatus.Rejected.ToString())
            {
                return;
            }

            entity.ApprovalStatus = OpexApprovalStatus.Rejected.ToString();
            entity.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _cache.Remove("OpexRequestCache");
        }

        // Approve
        // Approve
        public async Task<string> ApproveOpexRequest(int id, int approvedBy)
        {
            var opex = await _context.OpexRequests
                .FirstOrDefaultAsync(x => x.OpexRequestId == id);

            if (opex == null)
            {
                return "Opex Request Not Found";
            }

            if (opex.ApprovalStatus == OpexApprovalStatus.Rejected.ToString())
            {
                return "Record Already Deleted";
            }

            if (opex.ApprovalStatus == OpexApprovalStatus.Approved.ToString())
            {
                return "Opex Request Already Approved";
            }

            opex.ApprovalStatus = OpexApprovalStatus.Approved.ToString();
            opex.ApprovedBy = approvedBy;
            opex.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _cache.Remove("OpexRequestCache");

            return "Success";
        }
        // Reject
        public async Task<string> RejectOpexRequest(int id, int approvedBy)
        {
            var opex = await _context.OpexRequests
                .FirstOrDefaultAsync(x => x.OpexRequestId == id);

            if (opex == null)
            {
                return "Opex Request Not Found";
            }

            if (opex.ApprovalStatus == OpexApprovalStatus.Rejected.ToString())
            {
                return "Record Already Deleted";
            }

            if (opex.ApprovalStatus == OpexApprovalStatus.Rejected.ToString())
            {
                return "Opex Request Already Rejected";
            }

            opex.ApprovalStatus = OpexApprovalStatus.Rejected.ToString();
            opex.ApprovedBy = approvedBy;
            opex.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _cache.Remove("OpexRequestCache");

            return "Success";
        }

        // Summary
        // Summary
        public async Task<OpexSummaryDTO> GetOpexSummary()
        {
            OpexSummaryDTO summary = new OpexSummaryDTO();

            summary.TotalRequest = await _context.OpexRequests
                .CountAsync(x => x.ApprovalStatus != OpexApprovalStatus.Rejected.ToString());

            summary.ApprovedRequest = await _context.OpexRequests
                .CountAsync(x => x.ApprovalStatus == OpexApprovalStatus.Approved.ToString());

            summary.PendingRequest = await _context.OpexRequests
                .CountAsync(x => x.ApprovalStatus == OpexApprovalStatus.Pending.ToString());

            summary.RejectedRequest = await _context.OpexRequests
                .CountAsync(x => x.ApprovalStatus == OpexApprovalStatus.Rejected.ToString());

            summary.TotalAmount = await _context.OpexRequests
                .Where(x => x.ApprovalStatus != OpexApprovalStatus.Rejected.ToString())
                .SumAsync(x => x.Amount);

            return summary;
        }
        public async Task<List<OpexDropDownDTO>> GetBudgetLineDropdown()
        {
            return await _context.BudgetLines
                .Select(x => new OpexDropDownDTO
                {
                    Id = x.BudgetLineId,
                    Name = x.BudgetCategory.CategoryName
                })
                .ToListAsync();
        }
        public async Task<List<OpexDropDownDTO>> GetUserDropdown()
        {
            return await _context.Users
                .Select(x => new OpexDropDownDTO
                {
                    Id = x.UserId,
                    Name = x.FullName
                })
                .ToListAsync();
        }
    }
}