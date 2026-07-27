using Fincore.Application.CommonHelper;
using Fincore.Application.DTO;
using Fincore.Application.DTOs;
using Fincore.Application.DTOs.MasterTable;
using Fincore.Domain.Enums;

namespace Fincore.Application.Interfaces
{
    public interface ICountryService
    {
        Task<ApiResponse<List<CountryResponseDto>>> GetAllAsync(int page, int pageSize, string? countryName);
        Task<ApiResponse<CountryResponseDto>> GetByIdAsync(int id);

        Task<ApiResponse<CountryResponseDto>> CreateAsync(CountryDto dto);

        Task<ApiResponse<CountryResponseDto>> UpdateAsync(int id, CountryDto dto);

        Task<ApiResponse<string>> DeleteAsync(int id);
    }
}