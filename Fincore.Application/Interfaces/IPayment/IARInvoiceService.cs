using Fincore.Application.DTO;
using Fincore.Application.DTO.Payment.AccountsReceivable.Requests;
using Fincore.Application.DTO.Payment.AccountsReceivable.Responses;

namespace Fincore.Application.Interfaces.IPayment
{
    public interface IARInvoiceService
    {
        // Invoice

        Task<ApiResponse<ARInvoiceResponseDto>> CreateInvoiceAsync(
            CreateARInvoiceRequestDto request);

        Task<ApiResponse<List<ARInvoiceResponseDto>>> GetAllInvoicesAsync(
            int page,
            int pageSize);

        Task<ApiResponse<ARInvoiceResponseDto>> GetInvoiceByIdAsync(
            int arInvoiceId);

        Task<ApiResponse<ARInvoiceResponseDto>> UpdateInvoiceAsync(
            int arInvoiceId,
            UpdateARInvoiceRequestDto request);

        Task<ApiResponse<string>> DeleteInvoiceAsync(
            int arInvoiceId);

        // Payment

        Task<ApiResponse<ARPaymentResponseDto>> ReceivePaymentAsync(
            CreateARPaymentRequestDto request);

        // Reports

        Task<ApiResponse<List<AROutstandingResponseDto>>> GetOutstandingInvoicesAsync();

        Task<ApiResponse<ARAgingResponseDto>> GetAgingReportAsync();
    }
}