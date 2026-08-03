using Fincore.Application.DTO;
using Fincore.Application.DTO.Capex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.Interfaces.ICapex
{
    public interface IPurchaseOrderItemService
    {
        Task<ApiResponse<PurchaseOrderItemDTO>> AddPurchaseOrderItem(PurchaseOrderItemDTO dto);

        Task<ApiResponse<PurchaseOrderItemDTO>> GetPurchaseOrderItem(int id);

        Task<ApiResponse<List<PurchaseOrderItemDTO>>> GetAllPurchaseOrderItems(int page, int pageSize);

        Task<ApiResponse<PurchaseOrderItemDTO>> UpdatePurchaseOrderItem(int id, PurchaseOrderItemDTO dto);

        Task<ApiResponse<PurchaseOrderItemDTO>> DeletePurchaseOrderItem(int id);

        Task<ApiResponse<List<PurchaseOrderItemDTO>>> GetItemsByPOId(int poId);

        

    }
}
