using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;

namespace Fincore.Application.Interfaces.IMasterTable
{
    public interface IPermissionService
    {
        Task<ApiResponse<List<PermissionDto>>> GetAllPermissionsAsync(int pageNumber,int pageSize, string? search);

        Task<ApiResponse<PermissionDto>> GetPermissionByIdAsync(int id);

        Task<ApiResponse<PermissionDto>> CreatePermissionAsync(CreatePermissionDto createPermissionDto);

        Task<ApiResponse<PermissionDto>> UpdatePermissionAsync(int id,UpdatePermissionDto updatePermissionDto);

        Task<ApiResponse<bool>> DeletePermissionAsync(int id);
    }
}