using Fincore.Application.DTO;
using Fincore.Application.DTO.Reports;
using Fincore.Domain.Enums;

namespace Fincore.Application.Interfaces.Reports
{
    public interface IVendorSpendReportService
    {
        Task<ApiResponse<List<VendorSpendDTO>>> GetVendorSpendAsync( int page, int pageSize);

        Task<ApiResponse<List<VendorSpendDTO>>> GetVendorSpendByVendorAsync(int vendorId, int page,int pageSize);

        Task<ApiResponse<List<VendorSpendDTO>>> GetVendorSpendByPaymentStatusAsync( PaymentStatus paymentStatus,int page,int pageSize);

        Task<ApiResponse<List<VendorSpendDTO>>> GetVendorSpendByApprovalStatusAsync( ApprovalStatus approvalStatus,int page, int pageSize);

        Task<ApiResponse<List<VendorSpendDTO>>> GetVendorSpendByDateRangeAsync( DateTime fromDate,DateTime toDate, int page,int pageSize);
    }
}
