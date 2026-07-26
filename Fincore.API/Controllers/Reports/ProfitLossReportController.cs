using Fincore.Application.Interfaces.Reports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.Reports
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    public class ProfitLossReportController : ControllerBase
    {
        private readonly IProfitLossReportService service;

        public ProfitLossReportController(IProfitLossReportService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfitLoss(int? companyId,int? departmentId, DateTime? fromDate,DateTime? toDate)
        {
            var result = await service.GetProfitLossAsync(companyId, departmentId,fromDate,toDate);

            return Ok(result);
        }
    }
}
