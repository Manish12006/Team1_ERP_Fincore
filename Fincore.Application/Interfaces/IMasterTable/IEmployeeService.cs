using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;

namespace Fincore.Application.Interfaces.IMasterTable
{
    public interface IEmployeeService
    {
        Task<ApiResponse<List<EmployeeDto>>> GetAllEmployeesAsync(int pageNumber,int pageSize, string? search);

        Task<ApiResponse<EmployeeDto>> GetEmployeeByIdAsync(int id);

        Task<ApiResponse<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto);

        Task<ApiResponse<EmployeeDto>> UpdateEmployeeAsync(int id,UpdateEmployeeDto updateEmployeeDto);

        Task<ApiResponse<string>> DeleteEmployeeAsync(int id);
    }
}