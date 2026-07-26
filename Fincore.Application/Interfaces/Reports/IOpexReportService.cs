using Fincore.Application.DTO;
using Fincore.Application.DTO.Reports;
using System;
using Fincore.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.Interfaces.Reports
{
    public interface IOpexReportService
    {
        Task<ApiResponse<List<OpexReportDTO>>> GetOpexAsync(int page,int pageSize);

        Task<ApiResponse<List<OpexReportDTO>>> GetOpexByStatusAsync(ApprovalStatus approvalStatus, int page,int pageSize);

        Task<ApiResponse<List<OpexReportDTO>>> GetOpexByRequestedByAsync( int requestedBy,int page,int pageSize);
    }
}
