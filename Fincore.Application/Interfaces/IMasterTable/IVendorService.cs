using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;

namespace Fincore.Application.Interfaces.IMasterTable
{
    public interface IVendorService
    {
        Task<ApiResponse<List<VendorDto>>> GetAllVendorsAsync(int pageNumber,int pageSize, string? search);

        Task<ApiResponse<VendorDto>> GetVendorByIdAsync(int id);

        Task<ApiResponse<VendorDto>> CreateVendorAsync(CreateVendorDto createVendorDto);

        Task<ApiResponse<VendorDto>> UpdateVendorAsync(int id, UpdateVendorDto updateVendorDto);

        Task<ApiResponse<string>> DeleteVendorAsync(int id);
    }
}