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
    public interface IExpenseReportService
    {
        Task<ApiResponse<List<ExpenseReportDTO>>> GetExpenseAsync(int page, int pageSize);

        Task<ApiResponse<List<ExpenseReportDTO>>> GetExpenseByEmployeeAsync(int employeeId, int page, int pageSize);

        Task<ApiResponse<List<ExpenseReportDTO>>> GetExpenseByExpenseTypeAsync(ExpenseType expenseType, int page, int pageSize);

        Task<ApiResponse<List<ExpenseReportDTO>>> GetExpenseByStatusAsync(ApprovalStatus status, int page, int pageSize);

        Task<ApiResponse<List<ExpenseReportDTO>>> GetExpenseByDateRangeAsync(DateTime fromDate, DateTime toDate, int page, int pageSize);

        Task<ApiResponse<List<ExpenseReportDTO>>> GetExpenseByOpexRequestAsync(int opexRequestId, int page, int pageSize);
    }
}
