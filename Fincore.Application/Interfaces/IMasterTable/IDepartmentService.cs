using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;

namespace Fincore.Application.Interfaces.IMasterTable
{
    public interface IDepartmentService
    {
        Task<ApiResponse<IEnumerable<DepartmentDTO>>> GetAllDepartmentsAsync(int pageNumber,int pageSize, string? search);

        Task<ApiResponse<DepartmentDTO>> GetDepartmentByIdAsync(int id);

        Task<ApiResponse<DepartmentDTO>> CreateDepartmentAsync(DepartmentDTO departmentDTO);

        Task<ApiResponse<DepartmentDTO>> UpdateDepartmentAsync(int id,DepartmentDTO departmentDTO);

        Task<ApiResponse<bool>> DeleteDepartmentAsync(int id);
    }
}