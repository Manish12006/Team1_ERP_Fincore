using Fincore.Application.DTO;
using Fincore.Application.DTO.Capex;

namespace Fincore.Application.Interfaces.ICapex
{
    public interface IGRNService
    {
        // CRUD
        Task<ApiResponse<GRNDTO>> CreateGRN(GRNDTO dto);

        Task<ApiResponse<List<GRNDTO>>> GetAllGRN(int page, int pageSize);

        Task<ApiResponse<GRNDTO>> GetGRNById(int id);

        Task<ApiResponse<GRNDTO>> UpdateGRN(GRNDTO dto, int id);

        Task<ApiResponse<GRNDTO>> DeleteGRN(int id);


        // Filters

        Task<ApiResponse<List<GRNDTO>>> GetGRNByStatus(string status);

        Task<ApiResponse<List<GRNDTO>>> GetGRNByVendor(int vendorId);

        Task<ApiResponse<List<GRNDTO>>> GetGRNByPurchaseOrder(int poId);

        Task<ApiResponse<object>> GetGRNDropdown();

        // Actions

        Task<ApiResponse<GRNDTO>> ApproveQualityCheck(int id);

        Task<ApiResponse<GRNDTO>> RejectQualityCheck(int id);

        Task<ApiResponse<GRNDTO>> CloseGRN(int id);
        Task<ApiResponse<GRNDTO>> ReceiveGoods(GRNDTO dto);

        Task<ApiResponse<List<GRNDTO>>> GetGRNHistory(int id);
    }
}