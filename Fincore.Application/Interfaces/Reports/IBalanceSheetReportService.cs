using Fincore.Application.DTO;
using Fincore.Application.DTO.Reports;
using Fincore.Domain.Enums;

namespace Fincore.Application.Interfaces.Reports
{
    public interface IBalanceSheetReportService
    {
        Task<ApiResponse<List<BalanceSheetReportDTO>>> GetBalanceSheetAsync(int page, int pageSize);

        Task<ApiResponse<List<BalanceSheetReportDTO>>> GetBalanceSheetByAccountTypeAsync(AccountType accountType,int page, int pageSize);

        Task<ApiResponse<List<BalanceSheetReportDTO>>> GetBalanceSheetByDateRangeAsync(DateTime fromDate,DateTime toDate,int page, int pageSize);
    }
}