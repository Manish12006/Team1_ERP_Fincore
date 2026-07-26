using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;

namespace Fincore.Application.Interfaces.IMasterTable
{
    public interface IRoleService
    {
        Task<ApiResponse<List<RoleDto>>> GetAllRolesAsync(int pageNumber,int pageSize, string? search);

        Task<ApiResponse<RoleDto>> GetRoleByIdAsync(int id);

        Task<ApiResponse<RoleDto>> CreateRoleAsync(CreateRoleDto createRoleDto);

        Task<ApiResponse<RoleDto>> UpdateRoleAsync(int id,UpdateRoleDto updateRoleDto);

        Task<ApiResponse<bool>> DeleteRoleAsync(int id);
    }
}