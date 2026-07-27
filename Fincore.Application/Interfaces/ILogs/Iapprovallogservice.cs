using Fincore.Application.CommonHelper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Logs;

namespace Fincore.Application.Interfaces.Logs
{
    public interface IApprovalLogService
    {
        Task<ApiResponse<PagedResponse<ApprovalLogResponseDto>>> GetAllAsync(
            int page,
            int pageSize,
            string? entityName = null);


        Task<ApiResponse<ApprovalLogResponseDto>> GetByIdAsync(
            int id);


        Task<ApiResponse<ApprovalLogResponseDto>> CreateAsync(
            ApprovalLogDto dto);


        Task<ApiResponse<ApprovalLogResponseDto>> UpdateAsync(
            int id,
            ApprovalLogDto dto);


        Task<ApiResponse<bool>> DeleteAsync(
            int id);


        Task<List<object>> GetUsersDropdownAsync();
    }
}