using Fincore.Application.CommonHelper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Logs;

namespace Fincore.Application.Interfaces.Logs
{
    public interface IUserActivityLogService
    {
        Task<ApiResponse<PagedResponse<UserActivityLogResponseDto>>> GetAllAsync(
            int page,
            int pageSize,
            string? activityType = null);

        Task<ApiResponse<UserActivityLogResponseDto>> GetByIdAsync(
            long id);

        Task<ApiResponse<UserActivityLogResponseDto>> CreateAsync(
            UserActivityLogDto dto);

        Task<ApiResponse<UserActivityLogResponseDto>> UpdateAsync(
            long id,
            UserActivityLogDto dto);

        Task<ApiResponse<bool>> DeleteAsync(
            long id);

        Task<List<object>> GetUsersDropdownAsync();
    }
}