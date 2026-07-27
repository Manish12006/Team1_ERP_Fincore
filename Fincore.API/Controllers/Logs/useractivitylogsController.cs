using Fincore.Application.DTO.Logs;
using Fincore.Application.Interfaces.Logs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.Logs
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("FixedPolicy")]
    public class UserActivityLogsController : ControllerBase
    {
        private readonly IUserActivityLogService service;

        public UserActivityLogsController(
            IUserActivityLogService service)
        {
            this.service = service;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll(
            int page = 1,
            int pageSize = 10,
            string? activityType = null)
        {
            var result = await service.GetAllAsync(
                page,
                pageSize,
                activityType);

            if (!result.success)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            long id)
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
            UserActivityLogDto dto)
        {
            var result = await service.CreateAsync(dto);

            if (!result.success)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            long id,
            UserActivityLogDto dto)
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
            long id)
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