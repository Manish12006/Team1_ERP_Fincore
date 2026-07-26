using Fincore.Application.Interfaces.Reports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.Reports
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    public class CashFlowReportController : ControllerBase
    {
        private readonly ICashFlowReportService service;

        public CashFlowReportController(ICashFlowReportService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetCashFlow( int? companyId,int? departmentId, DateTime? fromDate,DateTime? toDate)
        {
            var result = await service.GetCashFlowAsync(companyId,departmentId,fromDate,toDate);

            return Ok(result);
        }
    }
}
