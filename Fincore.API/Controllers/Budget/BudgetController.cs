using Fincore.Application.DTOs.Budget;
using Fincore.Application.Interfaces.Budget;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers
{
    [ApiController]
    [Route("api/v1/[action]")]
    [EnableRateLimiting("FixedPolicy")]
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetService _budgetService;

        public BudgetController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        // Add
        [HttpPost]
        public async Task<IActionResult> AddBudget(CreateBudgetDTO dto)
        {
            var response = await _budgetService.AddBudget(dto);
            return Ok(response);
        }

        // Get All
        [HttpGet]
        public async Task<IActionResult> GetBudgets(
            string? budgetCode,
            string? budgetName,
            string? financialYear,
            int? budgetCategoryId,
            byte? isActive,
            int page = 1,
            int pageSize = 5)
        {

            if (page <= 0)
            {
                return BadRequest("Page must be greater than 0");
            }

            if (pageSize <= 0)
            {
                return BadRequest("Page Size must be greater than 0");
            }

            if (pageSize > 50)
            {
                return BadRequest("Maximum Page Size is 50");
            }
            var response = await _budgetService.GetBudgets(
                budgetCode,
                budgetName,
                financialYear,
                budgetCategoryId,
                isActive,
                page,
                pageSize);

            return Ok(response);
        }

        // Get By Id
        [HttpGet]
        public async Task<IActionResult> GetBudgetById(int id)
        {
            var response = await _budgetService.GetBudgetById(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }

        // Update
        [HttpPut]
        public async Task<IActionResult> UpdateBudget(int id, UpdateBudgetDTO dto)
        {
            var response = await _budgetService.UpdateBudget(id, dto);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }

        // Delete
        [HttpDelete]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            var response = await _budgetService.DeleteBudget(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }

        // Summary
        [HttpGet]
        public async Task<IActionResult> GetBudgetSummary()
        {
            var response = await _budgetService.GetBudgetSummary();
            return Ok(response);
        }
    }
}