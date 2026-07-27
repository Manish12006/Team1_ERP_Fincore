using Fincore.Application.CommonHelper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.Logs;

namespace Fincore.Application.Interfaces.Logs
{
    public interface IAuditLogService
    {
        Task<ApiResponse<PagedResponse<AuditLogResponseDto>>> GetAllAsync(
            int page,
            int pageSize,
            string? entityName = null);

        Task<ApiResponse<AuditLogResponseDto>> GetByIdAsync(
            long id);

        Task<ApiResponse<AuditLogResponseDto>> CreateAsync(
            AuditLogDto dto);

        Task<ApiResponse<AuditLogResponseDto>> UpdateAsync(
            long id,
            AuditLogDto dto);

        Task<ApiResponse<bool>> DeleteAsync(
            long id);
    }
}