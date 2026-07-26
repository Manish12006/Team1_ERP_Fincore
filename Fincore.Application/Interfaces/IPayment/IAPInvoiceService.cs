using Fincore.Application.DTO;
using Fincore.Application.DTO.Payment.AccountsReceivable.Requests;
using Fincore.Application.DTO.Payment.APInvoice.Requests;
using Fincore.Application.DTO.Payment.APInvoice.Responses;


namespace Fincore.Application.Interfaces.Payment
{
    public interface IAPInvoiceService
    {
        Task<ApiResponse<APInvoiceResponseDto>> CreateAsync(CreateAPInvoiceRequestDto request);

        Task<ApiResponse<List<APInvoiceResponseDto>>> GetAllAsync(APInvoiceFilterDto filter);

        Task<ApiResponse<APInvoiceResponseDto>> ApproveAsync(int id);

        Task<ApiResponse<APInvoiceResponseDto>> RecordPaymentAsync(CreatePaymentRequestDto request);


        Task<ApiResponse<APInvoiceResponseDto>> UpdateAsync(
                                                            int id,
                                                            UpdateAPInvoiceRequestDto request);

        Task<ApiResponse<string>> DeleteAsync(int id);

        Task<ApiResponse<List<APOutstandingDto>>> GetOutstandingAsync(APOutstandingFilterDto filter);

        Task<ApiResponse<APAgingReportDto>> GetAgingReportAsync(APAgingFilterDto filter);
    }
}