using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;

namespace Fincore.Application.Interfaces.IMasterTable
{
    public interface IVendorCategoryService
    {
        Task<ApiResponse<List<VendorCategoryDto>>> GetAllVendorCategoriesAsync(int pageNumber,int pageSize, string? search);

        Task<ApiResponse<VendorCategoryDto>> GetVendorCategoryByIdAsync(int id);

        Task<ApiResponse<VendorCategoryDto>> CreateVendorCategoryAsync(CreateVendorCategoryDto createVendorCategoryDto);

        Task<ApiResponse<VendorCategoryDto>> UpdateVendorCategoryAsync(int id, UpdateVendorCategoryDto updateVendorCategoryDto);

        Task<ApiResponse<string>> DeleteVendorCategoryAsync(int id);
    }
}