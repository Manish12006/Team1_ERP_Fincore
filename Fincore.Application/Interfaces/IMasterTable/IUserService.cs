using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;

namespace Fincore.Application.Interfaces.IMasterTable
{
    public interface IUserService
    {
        Task<ApiResponse<List<UserDto>>> GetAllUsersAsync(int pageNumber,int pageSize, string? search);

        Task<ApiResponse<UserDto>> GetUserByIdAsync(int id);

        Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto createUserDto);

        Task<ApiResponse<UserDto>> UpdateUserAsync(int id,UpdateUserDto updateUserDto);

        Task<ApiResponse<bool>> DeleteUserAsync(int id);
    }
}