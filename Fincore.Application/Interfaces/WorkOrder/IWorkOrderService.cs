using Fincore.Application.DTO;
using Fincore.Application.DTOs.WorkOrder;

namespace Fincore.Application.Interfaces.WorkOrder
{
    public interface IWorkOrderService
    {
    
        Task<ApiResponse<string>> AddWorkOrder(CreateWorkOrderDTO dto);
        Task<ApiResponse<List<WorkOrderResponseDTO>>> GetWorkOrders(
       string? title,
       int? vendorId,
       int? opexRequestId,
       string? status,
       int page,
       int pageSize);

        Task<ApiResponse<WorkOrderResponseDTO>> GetWorkOrderById(int id);

        Task<ApiResponse<string>> UpdateWorkOrder(int id, UpdateWorkOrderDTO dto);
        Task<ApiResponse<string>> DeleteWorkOrder(int id);

        Task<ApiResponse<string>> ApproveWorkOrder(int id);
        Task<ApiResponse<string>> RejectWorkOrder(int id);

        Task<ApiResponse<WorkOrderSummaryDTO>> GetWorkOrderSummary();
    }
}