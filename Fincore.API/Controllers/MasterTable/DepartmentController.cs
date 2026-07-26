using Fincore.Application.DTO.MasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.MasterTable
{
    [Route("api/v1/departments")]
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            this.departmentService = departmentService;
        }


       
        [HttpGet]
        public async Task<IActionResult> GetAllDepartments(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null)
        {
            var response = await departmentService
                .GetAllDepartmentsAsync(pageNumber, pageSize,search);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            var response = await departmentService
                .GetDepartmentByIdAsync(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }


        [HttpPost]
        public async Task<IActionResult> CreateDepartment(
            [FromBody] DepartmentDTO departmentDTO)
        {
            var response = await departmentService
                .CreateDepartmentAsync(departmentDTO);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(
            int id,
            [FromBody] DepartmentDTO departmentDTO)
        {
            var response = await departmentService
                .UpdateDepartmentAsync(id, departmentDTO);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }


       
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var response = await departmentService
                .DeleteDepartmentAsync(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }
    }
}
