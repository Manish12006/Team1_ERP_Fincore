using Fincore.Application.DTO;
using Fincore.Application.DTO.Capex;


namespace Fincore.Application.Interfaces.ICapex
{
    public interface IAssetService
    {


        Task<ApiResponse<AssetDTO>> AddAsset(AssetDTO dto);



        Task<ApiResponse<AssetDTO>> GetAsset(int id);



        Task<ApiResponse<List<AssetDTO>>> GetAllAssets(int page,int pageSize);



        Task<ApiResponse<AssetDTO>> UpdateAsset(int id,AssetDTO dto);



        Task<ApiResponse<AssetDTO>> DeleteAsset(int id);





        Task<ApiResponse<AssetDTO>> AssignAsset(int id,int userId);



        Task<ApiResponse<AssetDTO>> TransferAsset(int id,int departmentId);



        Task<ApiResponse<AssetDTO>> DisposeAsset(int id);



        Task<ApiResponse<AssetDTO>> RepairAsset(int id);



        Task<ApiResponse<AssetDTO>> ReturnAsset(int id);




        // Filter

        Task<ApiResponse<List<AssetDTO>>> GetAssetByStatus(string status);



        // Dropdown

        Task<ApiResponse<object>> GetAssetDropdown();
        Task<ApiResponse<object>> GetVendorDropdown();

        Task<ApiResponse<object>> GetDepartmentDropdown();

        Task<ApiResponse<object>> GetGRNDropdown();

        Task<ApiResponse<object>> GetPurchaseOrderDropdown();
    }
}