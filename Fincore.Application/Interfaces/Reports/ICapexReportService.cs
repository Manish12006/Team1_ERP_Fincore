using Fincore.Application.DTO;
using Fincore.Application.DTO.Reports;
using Fincore.Domain.Enums;

namespace Fincore.Application.Interfaces.Reports
{
    public interface ICapexReportService
    {
        Task<ApiResponse<List<CapexReportDTO>>> GetCapexAsync(int page, int pageSize);

        Task<ApiResponse<List<CapexReportDTO>>> GetCapexByDepartmentAsync(int departmentId,int page,int pageSize);

        Task<ApiResponse<List<CapexReportDTO>>> GetCapexByStatusAsync(ApprovalStatus approvalStatus,int page,int pageSize);
    }
}