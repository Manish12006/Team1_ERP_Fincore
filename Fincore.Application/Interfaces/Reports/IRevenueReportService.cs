using Fincore.Application.DTO;
using Fincore.Application.DTO.Reports;
using Fincore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.Interfaces.Reports
{
    public interface IRevenueReportService
    {
        Task<ApiResponse<List<RevenueReportDTO>>> GetRevenueAsync( int page,int pageSize);

        Task<ApiResponse<List<RevenueReportDTO>>> GetRevenueByCustomerAsync( int customerId,int page,int pageSize);

        Task<ApiResponse<List<RevenueReportDTO>>> GetRevenueByDepartmentAsync(int departmentId, int page,int pageSize);

        Task<ApiResponse<List<RevenueReportDTO>>> GetRevenueByRevenueTypeAsync(RevenueType revenueType,int page,int pageSize);

        Task<ApiResponse<List<RevenueReportDTO>>> GetRevenueByAccountAsync(int accountId,int page,int pageSize);

        Task<ApiResponse<List<RevenueReportDTO>>> GetRevenueByStatusAsync(RevenueStatus status, int page,int pageSize);

        Task<ApiResponse<List<RevenueReportDTO>>> GetRevenueByDateRangeAsync(DateTime fromDate,DateTime toDate,int page, int pageSize);
    }
}
