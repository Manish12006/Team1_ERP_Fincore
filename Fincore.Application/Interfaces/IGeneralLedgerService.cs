using Fincore.Application.DTO;
using Fincore.Application.DTO.GeneralLedger;

namespace Fincore.Application.Interfaces
{
    public interface IGeneralLedgerService
    {
        
        Task<ApiResponse<List<GeneralLedgerReadDTO>>> GetAllAsync(string? journalNumber,int? accountId,DateTime? fromDate, DateTime? toDate,string? description,int page,int pageSize);

        Task<ApiResponse<GeneralLedgerReadDTO>> GetByIdAsync(int id);

        Task<ApiResponse<GeneralLedgerSummaryDTO>> GetSummaryAsync();

        
        Task<ApiResponse<List<TrialBalanceReadDTO>>> GetTrialBalanceAsync();

        Task<ApiResponse<TrialBalanceSummaryDTO>> GetTrialBalanceSummaryAsync();

        
        Task<ApiResponse<List<LedgerAccountReadDTO>>> GetLedgerAccountAsync(int accountId,DateTime? fromDate,DateTime? toDate, int page,int pageSize);

       
        Task<ApiResponse<List<AccountingReportReadDTO>>> GetAccountingReportAsync(DateTime? fromDate,DateTime? toDate,int? accountId, int page, int pageSize);
    }
}