using Fincore.Application.DTO.MasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.MasterTable
{
    [Route("api/v1/permissions")]
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService permissionService;

        public PermissionsController(
            IPermissionService permissionService)
        {
            this.permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPermissions(
            int pageNumber = 1,
            int pageSize = 10,
             string? search=null)
        {
            var response = await permissionService
                .GetAllPermissionsAsync(pageNumber, pageSize,search);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPermissionById(int id)
        {
            var response = await permissionService
                .GetPermissionByIdAsync(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePermission(
            [FromBody] CreatePermissionDto createPermissionDto)
        {
            var response = await permissionService
                .CreatePermissionAsync(createPermissionDto);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermission(
            int id,
            [FromBody] UpdatePermissionDto updatePermissionDto)
        {
            var response = await permissionService
                .UpdatePermissionAsync(id, updatePermissionDto);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var response = await permissionService
                .DeletePermissionAsync(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }
    }
}