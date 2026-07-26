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
    public class BalanceSheetReportController : ControllerBase
    {
        private readonly IBalanceSheetReportService service;

        public BalanceSheetReportController(IBalanceSheetReportService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetBalanceSheet(int page = 1, int pageSize = 10)
        {
            var result = await service.GetBalanceSheetAsync(page, pageSize);
            return Ok(result);
        }

            [HttpGet("account-type/{accountType}")]
            public async Task<IActionResult> GetBalanceSheetByAccountType(AccountType accountType, int page = 1, int pageSize = 10)
            {
                var result = await service.GetBalanceSheetByAccountTypeAsync(accountType, page, pageSize);
                return Ok(result);
            }
}
}
