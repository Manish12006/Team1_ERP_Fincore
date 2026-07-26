using Fincore.Application.DTO;
using Fincore.Application.DTO.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.Interfaces.Reports
{
    public interface ICashFlowReportService
    {
        Task<ApiResponse<CashFlowReportDTO>> GetCashFlowAsync(int? companyId,int? departmentId,DateTime? fromDate, DateTime? toDate);
    }
}
