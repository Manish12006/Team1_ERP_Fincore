using Fincore.Application.DTOs.ExpenseClaim;
using Fincore.Application.Interfaces.ExpenseClaim;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers
{
    [ApiController]
    [Route("api/v1/expenseclaim/[action]")]
    [EnableRateLimiting("FixedPolicy")]
    public class ExpenseClaimController : ControllerBase
    {
        private readonly IExpenseClaimService _expenseClaimService;

        public ExpenseClaimController(IExpenseClaimService expenseClaimService)
        {
            _expenseClaimService = expenseClaimService;
        }

        // Create
        [HttpPost]
        public async Task<IActionResult> AddExpenseClaim(CreateExpenseClaimDTO dto)
        {
            var response = await _expenseClaimService.AddExpenseClaim(dto);

            if (!response.success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // Get All
        [HttpGet]
        public async Task<IActionResult> GetExpenseClaims(
        string? claimNumber,
        int? opexRequestId,
        int? claimBy,
        string? approvalStatus,
        int page = 1,
        int pageSize = 5)
        {
            // Page Validation
            if (page <= 0)
            {
                return BadRequest(new
                {
                    message = "Page number must be greater than 0"
                });
            }

            if (pageSize <= 0)
            {
                return BadRequest(new
                {
                    message = "Page Size must be greater than 0"
                });
            }

            if (pageSize > 50)
            {
                return BadRequest(new
                {
                    message = "Maximum Page Size is 50"
                });
            }

            var response = await _expenseClaimService.GetExpenseClaims(
                claimNumber,
                opexRequestId,
                claimBy,
                approvalStatus,
                page,
                pageSize);

            return Ok(response);
        }

        // Get By Id
        [HttpGet]
        public async Task<IActionResult> GetExpenseClaimById(int id)
        {
            var response = await _expenseClaimService.GetExpenseClaimById(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }

        // Update
        [HttpPut]
        public async Task<IActionResult> UpdateExpenseClaim(int id, UpdateExpenseClaimDTO dto)
        {
            var response = await _expenseClaimService.UpdateExpenseClaim(id, dto);

            if (!response.success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // Delete
        [HttpDelete]
        public async Task<IActionResult> DeleteExpenseClaim(int id)
        {
            var response = await _expenseClaimService.DeleteExpenseClaim(id);

            if (!response.success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // Approve
        [HttpPost]
        public async Task<IActionResult> ApproveExpenseClaim(int id, int approvedBy)
        {
            var response = await _expenseClaimService.ApproveExpenseClaim(id, approvedBy);

            if (!response.success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // Reject
        [HttpPost]
        public async Task<IActionResult> RejectExpenseClaim(int id, int approvedBy)
        {
            var response = await _expenseClaimService.RejectExpenseClaim(id, approvedBy);

            if (!response.success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // Summary
        [HttpGet]
        public async Task<IActionResult> GetExpenseClaimSummary()
        {
            var response = await _expenseClaimService.GetExpenseClaimSummary();
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetOpexRequestDropdown()
        {
            var data = await _expenseClaimService.GetOpexRequestDropdown();

            return Ok(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserDropdown()
        {
            var data = await _expenseClaimService.GetUserDropdown();

            return Ok(data);
        }
    }
}