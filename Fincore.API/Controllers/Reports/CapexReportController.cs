using Fincore.Application.Interfaces.Reports;
using Fincore.Infrastructure.Services.Capex;
using Fincore.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.Reports
{

    [Route("api/v1/[controller]")]
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    public class CapexReportController : ControllerBase
    {
        private readonly ICapexReportService service;

        public CapexReportController(ICapexReportService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetCapex(int page = 1,int pageSize = 10)
        {
            var result = await service.GetCapexAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> GetCapexByDepartment(int departmentId,int page = 1,int pageSize = 10)
        {
            var result = await service.GetCapexByDepartmentAsync(departmentId,page,pageSize);

            return Ok(result);
        }

       

        [HttpGet("status/{approvalStatus}")]
            public async Task<IActionResult> GetCapexByStatus(ApprovalStatus approvalStatus, int page = 1,int pageSize = 10)
            {
                var result = await service.GetCapexByStatusAsync(approvalStatus, page, pageSize);
                return Ok(result);
            }
}
}
