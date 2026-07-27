using Fincore.Application.CommonHelper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Logs;

namespace Fincore.Application.Interfaces.Logs
{
    public interface INotificationLogService
    {
        Task<ApiResponse<PagedResponse<NotificationLogResponseDto>>> GetAllAsync(
            int page,
            int pageSize,
            string? title = null);


        Task<ApiResponse<NotificationLogResponseDto>> GetByIdAsync(
            long id);


        Task<ApiResponse<NotificationLogResponseDto>> CreateAsync(
            NotificationLogDto dto);


        Task<ApiResponse<NotificationLogResponseDto>> UpdateAsync(
            long id,
            NotificationLogDto dto);


        Task<ApiResponse<bool>> DeleteAsync(
            long id);


        Task<List<object>> GetUsersDropdownAsync();
    }
}