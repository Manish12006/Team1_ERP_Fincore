using Fincore.Application.Interfaces.Reports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.Reports
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    public class BudgetVarianceReportController : ControllerBase
    {
        private readonly IBudgetVarianceReportService service;

        public BudgetVarianceReportController(IBudgetVarianceReportService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetBudgetVariance(int page = 1,int pageSize = 10)
        {
            var result = await service.GetBudgetVarianceAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> GetBudgetVarianceByDepartment( int departmentId, int page = 1,int pageSize = 10)
        {
            var result = await service.GetBudgetVarianceByDepartmentAsync(departmentId,page,pageSize);

            return Ok(result);
        }

        [HttpGet("financial-year/{financialYear}")]
        public async Task<IActionResult> GetBudgetVarianceByFinancialYear( string financialYear, int page = 1,int pageSize = 10)
        {
            var result = await service.GetBudgetVarianceByFinancialYearAsync(financialYear, page,pageSize);

            return Ok(result);
        }
    }
}
