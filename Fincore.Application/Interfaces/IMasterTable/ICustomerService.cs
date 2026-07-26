using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;

namespace Fincore.Application.Interfaces.IMasterTable
{
    public interface ICustomerService
    {
        Task<ApiResponse<List<CustomerDto>>> GetAllCustomersAsync(int pageNumber,int pageSize, string? search);

        Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(int id);

        Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CreateCustomerDto createCustomerDto);

        Task<ApiResponse<CustomerDto>> UpdateCustomerAsync(int id,UpdateCustomerDto updateCustomerDto);

        Task<ApiResponse<bool>> DeleteCustomerAsync(int id);
    }
}