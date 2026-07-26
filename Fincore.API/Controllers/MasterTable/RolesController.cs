using Fincore.Application.DTO.MasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.MasterTable
{
    [Route("api/v1/roles")]
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService roleService;

        public RolesController(IRoleService roleService)
        {
            this.roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles(
            int pageNumber = 1,
            int pageSize = 10,
            string? search=null)
        {
            var response = await roleService
                .GetAllRolesAsync(pageNumber, pageSize,search);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            var response = await roleService
                .GetRoleByIdAsync(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(
            [FromBody] CreateRoleDto createRoleDto)
        {
            var response = await roleService
                .CreateRoleAsync(createRoleDto);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(
            int id,
            [FromBody] UpdateRoleDto updateRoleDto)
        {
            var response = await roleService
                .UpdateRoleAsync(id, updateRoleDto);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var response = await roleService
                .DeleteRoleAsync(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }
    }
}