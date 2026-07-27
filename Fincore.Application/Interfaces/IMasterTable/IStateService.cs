using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;
using Fincore.Application.DTOs.MasterTable;

namespace Fincore.Application.Interfaces.IMasterTable
{
    public interface IStateService
    {
        Task<ApiResponse<List<StateResponseDto>>> GetAllAsync(int page, int pageSize, string? stateName);

        Task<ApiResponse<StateResponseDto>> GetByIdAsync(int id);

        Task<ApiResponse<StateResponseDto>> CreateAsync(StateDto dto);

        Task<ApiResponse<StateResponseDto>> UpdateAsync(int id, StateDto dto);

        Task<ApiResponse<string>> DeleteAsync(int id);
    }
}