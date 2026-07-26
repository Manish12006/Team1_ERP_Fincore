using Fincore.Application.DTO.MasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.MasterTable
{
    [Route("api/v1/users")]
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService userService;

        public UsersController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers(
            int pageNumber = 1,
            int pageSize = 10,
            string? search=null)
        {
            var response = await userService
                .GetAllUsersAsync(pageNumber, pageSize,search);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var response = await userService
                .GetUserByIdAsync(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }


        [HttpPost]
        public async Task<IActionResult> CreateUser(
            [FromBody] CreateUserDto createUserDto)
        {
            var response = await userService
                .CreateUserAsync(createUserDto);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(
            int id,
            [FromBody] UpdateUserDto updateUserDto)
        {
            var response = await userService
                .UpdateUserAsync(id, updateUserDto);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var response = await userService
                .DeleteUserAsync(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }
    }
}