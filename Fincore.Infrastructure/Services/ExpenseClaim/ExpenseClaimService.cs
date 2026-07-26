using AutoMapper;
using Fincore.Application.DTO;
using Fincore.Application.DTOs.Common;
using Fincore.Application.DTOs.ExpenseClaim;
using Fincore.Application.Interfaces.ExpenseClaim;
using Fincore.Domain.Enums;
using Fincore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ExpenseClaimModel = Fincore.Domain.Models.ExpenseClaim;

namespace Fincore.Infrastructure.Services.ExpenseClaim
{ 
    public class ExpenseClaimService : IExpenseClaimService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public ExpenseClaimService
        (
            AppDbContext context,
            IMapper mapper,
            IMemoryCache cache
        )
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }
        public async Task<ApiResponse<string>> AddExpenseClaim(CreateExpenseClaimDTO dto)
        {
            ApiResponse<string> response = new ApiResponse<string>();

            // Opex Request Validation
            var opex = await _context.OpexRequests
                .FirstOrDefaultAsync(x => x.OpexRequestId == dto.OpexRequestId);

            if (opex == null)
            {
                response.success = false;
                response.message = "Opex Request Not Found";
                return response;
            }

            // User Validation
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == dto.ClaimBy);

            if (user == null)
            {
                response.success = false;
                response.message = "User Not Found";
                return response;
            }

            // Claim Number Validation
            if (string.IsNullOrWhiteSpace(dto.ClaimNumber))
            {
                response.success = false;
                response.message = "Claim Number is Required";
                return response;
            }

            // Description Validation
            if (string.IsNullOrWhiteSpace(dto.Description))
            {
                response.success = false;
                response.message = "Description is Required";
                return response;
            }

            // Amount Validation
            if (dto.ExpenseAmount <= 0)
            {
                response.success = false;
                response.message = "Expense Amount must be greater than zero";
                return response;
            }

            // Create Entity
            var entity = _mapper.Map<Fincore.Domain.Models.ExpenseClaim>(dto);

            entity.ApprovalStatus = OpexApprovalStatus.Pending.ToString();
            entity.CreatedAt = DateTime.Now;

            await _context.ExpenseClaims.AddAsync(entity);
            await _context.SaveChangesAsync();

            _cache.Remove("ExpenseClaimList");

            response.success = true;
            response.message = "Expense Claim Added Successfully";
            response.data = "Success";

            return response;
        }
        public async Task<ApiResponse<List<ExpenseClaimResponseDTO>>> GetExpenseClaims(
            string? claimNumber,
            int? opexRequestId,
            int? claimBy,
            string? approvalStatus,
            int page,
            int pageSize)
        {
            ApiResponse<List<ExpenseClaimResponseDTO>> response =
                new ApiResponse<List<ExpenseClaimResponseDTO>>();

            string cacheKey =
                $"ExpenseClaim_{claimNumber}_{opexRequestId}_{claimBy}_{approvalStatus}_{page}_{pageSize}";

            if (!_cache.TryGetValue(cacheKey, out List<ExpenseClaimResponseDTO> data))
            {
                var query = _context.ExpenseClaims
                    .Where(x => x.ApprovalStatus != OpexApprovalStatus.Rejected.ToString())
                    .AsQueryable();

                if (!string.IsNullOrEmpty(claimNumber))
                {
                    query = query.Where(x => x.ClaimNumber.Contains(claimNumber));
                }

                if (opexRequestId.HasValue)
                {
                    query = query.Where(x => x.OpexRequestId == opexRequestId);
                }

                if (claimBy.HasValue)
                {
                    query = query.Where(x => x.ClaimBy == claimBy);
                }

                if (!string.IsNullOrEmpty(approvalStatus))
                {
                    query = query.Where(x => x.ApprovalStatus == approvalStatus);
                }

                var list = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                data = _mapper.Map<List<ExpenseClaimResponseDTO>>(list);

                _cache.Set(cacheKey, data, TimeSpan.FromMinutes(5));
            }

            response.success = true;
            response.message = "Expense Claims Fetched Successfully";
            response.data = data;
            response.totalNumberRecord = data.Count;

            return response;
        }
        public async Task<ApiResponse<ExpenseClaimResponseDTO>> GetExpenseClaimById(int id)
        {
            ApiResponse<ExpenseClaimResponseDTO> response =
                new ApiResponse<ExpenseClaimResponseDTO>();

            string cacheKey = $"ExpenseClaim_{id}";

            if (!_cache.TryGetValue(cacheKey, out ExpenseClaimResponseDTO dto))
            {
                var entity = await _context.ExpenseClaims
                    .FirstOrDefaultAsync(x =>
                        x.ExpenseClaimId == id &&
                        x.ApprovalStatus != OpexApprovalStatus.Rejected.ToString());

                if (entity == null)
                {
                    response.success = false;
                    response.message = "Expense Claim Not Found";
                    return response;
                }

                dto = _mapper.Map<ExpenseClaimResponseDTO>(entity);

                _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));
            }

            response.success = true;
            response.message = "Expense Claim Found Successfully";
            response.data = dto;

            return response;
        }
        public async Task<ApiResponse<string>> UpdateExpenseClaim(int id, UpdateExpenseClaimDTO dto)
        {
            ApiResponse<string> response = new ApiResponse<string>();

            var entity = await _context.ExpenseClaims
                .FirstOrDefaultAsync(x => x.ExpenseClaimId == id);

            if (entity == null)
            {
                response.success = false;
                response.message = "Expense Claim Not Found";
                return response;
            }

            if (entity.ApprovalStatus == OpexApprovalStatus.Rejected.ToString())
            {
                response.success = false;
                response.message = "Record Already Deleted";
                return response;
            }

            if (string.IsNullOrWhiteSpace(dto.Description))
            {
                response.success = false;
                response.message = "Description is Required";
                return response;
            }

            if (dto.ExpenseAmount <= 0)
            {
                response.success = false;
                response.message = "Expense Amount must be greater than zero";
                return response;
            }

            _mapper.Map(dto, entity);

            entity.ModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _cache.Remove("ExpenseClaimList");
            _cache.Remove($"ExpenseClaim_{id}");

            response.success = true;
            response.message = "Expense Claim Updated Successfully";
            response.data = "Success";

            return response;
        }
        public async Task<ApiResponse<string>> DeleteExpenseClaim(int id)
        {
            ApiResponse<string> response = new ApiResponse<string>();

            var entity = await _context.ExpenseClaims
                .FirstOrDefaultAsync(x => x.ExpenseClaimId == id);

            if (entity == null)
            {
                response.success = false;
                response.message = "Expense Claim Not Found";
                return response;
            }

            if (entity.ApprovalStatus == OpexApprovalStatus.Rejected.ToString())
            {
                response.success = false;
                response.message = "Record Already Deleted";
                return response;
            }

            entity.ApprovalStatus = OpexApprovalStatus.Rejected.ToString();
            entity.ModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _cache.Remove("ExpenseClaimList");
            _cache.Remove($"ExpenseClaim_{id}");

            response.success = true;
            response.message = "Expense Claim Deleted Successfully";
            response.data = "Success";

            return response;
        }
        public async Task<ApiResponse<string>> ApproveExpenseClaim(int id, int approvedBy)
        {
            ApiResponse<string> response = new ApiResponse<string>();

            var entity = await _context.ExpenseClaims
                .FirstOrDefaultAsync(x => x.ExpenseClaimId == id);

            if (entity == null)
            {
                response.success = false;
                response.message = "Expense Claim Not Found";
                return response;
            }

            if (entity.ApprovalStatus == OpexApprovalStatus.Rejected.ToString())
            {
                response.success = false;
                response.message = "Record Already Deleted";
                return response;
            }

            entity.ApprovalStatus = OpexApprovalStatus.Approved.ToString();
            entity.ApprovedBy = approvedBy;
            entity.ModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _cache.Remove("ExpenseClaimList");
            _cache.Remove($"ExpenseClaim_{id}");

            response.success = true;
            response.message = "Expense Claim Approved Successfully";
            response.data = "Success";

            return response;
        }
        public async Task<ApiResponse<string>> RejectExpenseClaim(int id, int approvedBy)
        {
            ApiResponse<string> response = new ApiResponse<string>();

            var entity = await _context.ExpenseClaims
                .FirstOrDefaultAsync(x => x.ExpenseClaimId == id);

            if (entity == null)
            {
                response.success = false;
                response.message = "Expense Claim Not Found";
                return response;
            }

            if (entity.ApprovalStatus == OpexApprovalStatus.Rejected.ToString())
            {
                response.success = false;
                response.message = "Record Already Deleted";
                return response;
            }

            entity.ApprovalStatus = OpexApprovalStatus.Rejected.ToString();
            entity.ApprovedBy = approvedBy;
            entity.ModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            _cache.Remove("ExpenseClaimList");
            _cache.Remove($"ExpenseClaim_{id}");

            response.success = true;
            response.message = "Expense Claim Rejected Successfully";
            response.data = "Success";

            return response;
        }
        public async Task<ApiResponse<ExpenseClaimSummaryDTO>> GetExpenseClaimSummary()
        {
            ApiResponse<ExpenseClaimSummaryDTO> response =
                new ApiResponse<ExpenseClaimSummaryDTO>();

            ExpenseClaimSummaryDTO summary = new ExpenseClaimSummaryDTO();

            summary.TotalClaims = await _context.ExpenseClaims
                .CountAsync(x => x.ApprovalStatus != OpexApprovalStatus.Rejected.ToString());

            summary.ApprovedClaims = await _context.ExpenseClaims
                .CountAsync(x => x.ApprovalStatus == OpexApprovalStatus.Approved.ToString());

            summary.RejectedClaims = await _context.ExpenseClaims
                .CountAsync(x => x.ApprovalStatus == OpexApprovalStatus.Rejected.ToString());

            summary.PendingClaims = await _context.ExpenseClaims
                .CountAsync(x => x.ApprovalStatus == OpexApprovalStatus.Pending.ToString());

            summary.TotalExpenseAmount = await _context.ExpenseClaims
                .Where(x => x.ApprovalStatus != OpexApprovalStatus.Rejected.ToString())
                .SumAsync(x => x.ExpenseAmount);

            response.success = true;
            response.message = "Expense Claim Summary Fetched Successfully";
            response.data = summary;

            return response;
        }
        public async Task<List<OpexDropDownDTO>> GetOpexRequestDropdown()
        {
            return await _context.OpexRequests
                .Where(x => x.ApprovalStatus != OpexApprovalStatus.Rejected.ToString())
                .Select(x => new OpexDropDownDTO
                {
                    Id = x.OpexRequestId,
                    Name = x.Title
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