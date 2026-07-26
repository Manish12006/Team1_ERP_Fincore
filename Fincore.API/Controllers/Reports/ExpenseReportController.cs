using Fincore.Application.Interfaces.Reports;
using Fincore.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.Reports
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    public class ExpenseReportController : ControllerBase
    {
        private readonly IExpenseReportService service;

        public ExpenseReportController(IExpenseReportService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetExpense(int page = 1, int pageSize = 10)
        {
            var result = await service.GetExpenseAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetExpenseByEmployee(int employeeId, int page = 1, int pageSize = 10)
        {
            var result = await service.GetExpenseByEmployeeAsync(employeeId, page, pageSize);
            return Ok(result);
        }

        [HttpGet("type/{expenseType}")]
        public async Task<IActionResult> GetExpenseByExpenseType(ExpenseType expenseType, int page = 1, int pageSize = 10)
        {
            var result = await service.GetExpenseByExpenseTypeAsync(expenseType, page, pageSize);
            return Ok(result);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetExpenseByStatus(ApprovalStatus status, int page = 1, int pageSize = 10)
        {
            var result = await service.GetExpenseByStatusAsync(status, page, pageSize);
            return Ok(result);
        }

        [HttpGet("date-range")]
        public async Task<IActionResult> GetExpenseByDateRange(
            DateTime fromDate,
            DateTime toDate,
            int page = 1,
            int pageSize = 10)
        {
            var result = await service.GetExpenseByDateRangeAsync(fromDate, toDate, page, pageSize);
            return Ok(result);
        }

        [HttpGet("opex/{opexRequestId}")]
        public async Task<IActionResult> GetExpenseByOpexRequest(int opexRequestId, int page = 1, int pageSize = 10)
        {
            var result = await service.GetExpenseByOpexRequestAsync(opexRequestId, page, pageSize);
            return Ok(result);
        }
    }
}
