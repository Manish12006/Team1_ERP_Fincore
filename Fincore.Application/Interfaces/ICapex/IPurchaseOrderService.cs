using Fincore.Application.DTO;
using Fincore.Application.DTO.Capex;

namespace Fincore.Application.Interfaces.ICapex
{
    public interface IPurchaseOrderService
    {

        Task<ApiResponse<PurchaseOrderDTO>> AddPurchaseOrder(
            PurchaseOrderDTO dto);


        Task<ApiResponse<PurchaseOrderDTO>> GetPurchaseOrder(
            int id);


        Task<ApiResponse<List<PurchaseOrderDTO>>> GetAllPurchaseOrder(
            int page,
            int pageSize);


        Task<ApiResponse<List<PurchaseOrderDTO>>> GetPurchaseOrderByStatus(
            string status,
            int page,
            int pageSize);



        Task<ApiResponse<List<PurchaseOrderDTO>>> FilterPurchaseOrders(
            PurchaseOrderFilterDTO filter);



        Task<ApiResponse<PurchaseOrderDTO>> UpdatePurchaseOrder(
            int id,
            PurchaseOrderDTO dto);



        Task<ApiResponse<PurchaseOrderDTO>> DeletePurchaseOrder(
            int id);



        // Workflow APIs

        Task<ApiResponse<PurchaseOrderDTO>> ApprovePurchaseOrder(
            int id);


        Task<ApiResponse<PurchaseOrderDTO>> CancelPurchaseOrder(
            int id);


        Task<ApiResponse<PurchaseOrderDTO>> ClosePurchaseOrder(
            int id);



        // Dropdown API

        Task<ApiResponse<object>> GetPurchaseOrderDropdowns();

        Task<ApiResponse<List<DropdownDTO>>> GetVendorDropdown();

        Task<ApiResponse<List<DropdownDTO>>> GetPurchaseRequisitionDropdown();

        Task<ApiResponse<List<DropdownDTO>>> GetQuotationDropdown();

        Task<ApiResponse<List<DropdownDTO>>> GetUserDropdown();

        Task<ApiResponse<List<DropdownDTO>>> GetApprovalStatusDropdown();

        // Missing Business API

        Task<ApiResponse<List<PurchaseOrderDTO>>>
            GetPendingPurchaseOrders();



        Task<byte[]> GeneratePurchaseOrderPdf(int id);

    }
}