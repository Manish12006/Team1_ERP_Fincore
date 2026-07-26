using Fincore.Application.Interfaces.Reports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Fincore.Domain.Enums;

namespace Fincore.API.Controllers.Reports
{

    [Route("api/v1/[controller]")]
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    public class OpexReportController : ControllerBase
    {
        private readonly IOpexReportService service;

        public OpexReportController(IOpexReportService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetOpex(int page = 1,int pageSize = 10)
        {
            var result = await service.GetOpexAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("status/{approvalStatus}")]
        public async Task<IActionResult> GetOpexByStatus(ApprovalStatus approvalStatus,int page = 1,int pageSize = 10)
        {
            var result = await service.GetOpexByStatusAsync(approvalStatus, page, pageSize);
            return Ok(result);
        }

        [HttpGet("requested-by/{requestedBy}")]
        public async Task<IActionResult> GetOpexByRequestedBy(int requestedBy,int page = 1,int pageSize = 10)
        {
            var result = await service.GetOpexByRequestedByAsync(requestedBy, page,pageSize);

            return Ok(result);
        }
    }
}
