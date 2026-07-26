using Fincore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fincore.API.Controllers
{
    [ApiController]
    [Route("api/v1/general-ledger")]
    public class GeneralLedgerController : ControllerBase
    {
        private readonly IGeneralLedgerService service;

        public GeneralLedgerController(IGeneralLedgerService service)
        {
            this.service = service;
        }

        // Get General Ledger
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? journalNumber,
            int? accountId,
            DateTime? fromDate,
            DateTime? toDate,
            string? description,
            int page = 1,
            int pageSize = 10)
        {
            var result = await service.GetAllAsync(
                journalNumber,
                accountId,
                fromDate,
                toDate,
                description,
                page,
                pageSize);

            return Ok(result);
        }

        // Get General Ledger By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await service.GetByIdAsync(id);
            return Ok(result);
        }

        // General Ledger Summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await service.GetSummaryAsync();
            return Ok(result);
        }

        // Trial Balance
        [HttpGet("trial-balance")]
        public async Task<IActionResult> GetTrialBalance()
        {
            var result = await service.GetTrialBalanceAsync();
            return Ok(result);
        }

        // Trial Balance Summary
        [HttpGet("trial-balance/summary")]
        public async Task<IActionResult> GetTrialBalanceSummary()
        {
            var result = await service.GetTrialBalanceSummaryAsync();
            return Ok(result);
        }

        // Ledger Account
        [HttpGet("accounts/{accountId}")]
        public async Task<IActionResult> GetLedgerAccount(
            int accountId,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            int pageSize = 10)
        {
            var result = await service.GetLedgerAccountAsync(
                accountId,
                fromDate,
                toDate,
                page,
                pageSize);

            return Ok(result);
        }

        // Accounting Report
        [HttpGet("accounting-reports")]
        public async Task<IActionResult> GetAccountingReport(
            DateTime? fromDate,
            DateTime? toDate,
            int? accountId,
            int page = 1,
            int pageSize = 10)
        {
            var result = await service.GetAccountingReportAsync(
                fromDate,
                toDate,
                accountId,
                page,
                pageSize);

            return Ok(result);
        }
    }
}