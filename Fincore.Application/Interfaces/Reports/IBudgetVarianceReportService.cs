using Fincore.Application.DTO;
using Fincore.Application.DTO.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.Interfaces.Reports
{
    public interface IBudgetVarianceReportService
    {
        Task<ApiResponse<List<BudgetVarianceReportDTO>>> GetBudgetVarianceAsync(int page,int pageSize);

        Task<ApiResponse<List<BudgetVarianceReportDTO>>> GetBudgetVarianceByDepartmentAsync(int departmentId, int page, int pageSize);

        Task<ApiResponse<List<BudgetVarianceReportDTO>>> GetBudgetVarianceByFinancialYearAsync(string financialYear,int page, int pageSize);
    }
}
