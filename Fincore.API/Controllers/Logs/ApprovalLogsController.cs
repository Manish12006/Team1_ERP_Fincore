using Fincore.Application.DTO.Logs;
using Fincore.Application.Interfaces.Logs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.Logs
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("FixedPolicy")]
    public class ApprovalLogsController : ControllerBase
    {
        private readonly IApprovalLogService service;

        public ApprovalLogsController(
            IApprovalLogService service)
        {
            this.service = service;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll(
            int page = 1,
            int pageSize = 10,
            string? entityName = null)
        {
            var result = await service.GetAllAsync(
                page,
                pageSize,
                entityName);

            if (!result.success)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            int id)
        {
            var result = await service.GetByIdAsync(id);

            if (!result.success)
                return NotFound(result);

            return Ok(result);
        }


        [HttpGet("users-dropdown")]
        public async Task<IActionResult> GetUsersDropdown()
        {
            var result = await service.GetUsersDropdownAsync();

            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> Create(
            ApprovalLogDto dto)
        {
            var result = await service.CreateAsync(dto);

            if (!result.success)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            ApprovalLogDto dto)
        {
            var result = await service.UpdateAsync(
                id,
                dto);

            if (!result.success)
            {
                if (result.error?.Code == "NOT_FOUND")
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id)
        {
            var result = await service.DeleteAsync(id);

            if (!result.success)
            {
                if (result.error?.Code == "NOT_FOUND")
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}