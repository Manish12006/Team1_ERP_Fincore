using Fincore.Application.CommonHelper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;

namespace Fincore.Application.Interfaces.IMasterTable
{
    public interface ICityService
    {
        Task<ApiResponse<PagedResponse<CityResponseDto>>> GetAllAsync(
            int page,
            int pageSize,
            string? cityName = null);

        Task<ApiResponse<CityResponseDto>> GetByIdAsync(int id);

        Task<ApiResponse<CityResponseDto>> CreateAsync(CityDto dto);

        Task<ApiResponse<CityResponseDto>> UpdateAsync(
            int id,
            CityDto dto);

        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}