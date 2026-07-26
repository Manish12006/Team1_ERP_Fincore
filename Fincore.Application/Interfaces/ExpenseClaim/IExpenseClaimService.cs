using Fincore.Application.DTO;
using Fincore.Application.DTOs.Common;
using Fincore.Application.DTOs.ExpenseClaim;

namespace Fincore.Application.Interfaces.ExpenseClaim
{
    public interface IExpenseClaimService
    {
        // Create
        Task<ApiResponse<string>> AddExpenseClaim(CreateExpenseClaimDTO dto);
        Task<ApiResponse<List<ExpenseClaimResponseDTO>>> GetExpenseClaims(
     string? claimNumber,
     int? opexRequestId,
     int? claimBy,
     string? approvalStatus,
     int page,
     int pageSize);

        Task<ApiResponse<ExpenseClaimResponseDTO>> GetExpenseClaimById(int id);

        Task<ApiResponse<string>> UpdateExpenseClaim(int id, UpdateExpenseClaimDTO dto);

        Task<ApiResponse<string>> DeleteExpenseClaim(int id);

        Task<ApiResponse<string>> ApproveExpenseClaim(int id, int approvedBy);

        Task<ApiResponse<string>> RejectExpenseClaim(int id, int approvedBy);
        Task<ApiResponse<ExpenseClaimSummaryDTO>> GetExpenseClaimSummary();
        Task<List<OpexDropDownDTO>> GetOpexRequestDropdown();

        Task<List<OpexDropDownDTO>> GetUserDropdown();
    }
}