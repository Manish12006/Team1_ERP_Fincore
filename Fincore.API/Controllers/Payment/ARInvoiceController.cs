using Fincore.Application.DTO.Payment.AccountsReceivable.Requests;
using Fincore.Application.DTO.Payment.APInvoice.Requests;
using Fincore.Application.Interfaces.IPayment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FINCORE.API.Controllers.V1
{
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    [Route("api/v1/ar")]
    public class ARInvoiceController : ControllerBase
    {
        private readonly IARInvoiceService arInvoiceService;

        public ARInvoiceController(IARInvoiceService arInvoiceService)
        {
            this.arInvoiceService = arInvoiceService;
        }

        // Create Invoice
        [HttpPost("invoices")]
        public async Task<IActionResult> CreateInvoice(
            [FromBody] CreateARInvoiceRequestDto request)
        {
            var result = await arInvoiceService.CreateInvoiceAsync(request);
            return Ok(result);
        }

        // Get All Invoices
        [HttpGet("invoices")]
        public async Task<IActionResult> GetAllInvoices(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await arInvoiceService.GetAllInvoicesAsync(page, pageSize);
            return Ok(result);
        }

        // Get Invoice By Id
        [HttpGet("invoices/{id:int}")]
        public async Task<IActionResult> GetInvoiceById(int id)
        {
            var result = await arInvoiceService.GetInvoiceByIdAsync(id);
            return Ok(result);
        }

        // Update Invoice
        [HttpPut("invoices/{id:int}")]
        public async Task<IActionResult> UpdateInvoice(
            int id,
            [FromBody] UpdateARInvoiceRequestDto request)
        {
            var result = await arInvoiceService.UpdateInvoiceAsync(id, request);
            return Ok(result);
        }

        // Delete Invoice
        [HttpDelete("invoices/{id:int}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var result = await arInvoiceService.DeleteInvoiceAsync(id);
            return Ok(result);
        }

        // Receive Payment
        [HttpPost("payments")]
        public async Task<IActionResult> ReceivePayment(
            [FromBody] CreateARPaymentRequestDto request)
        {
            var result = await arInvoiceService.ReceivePaymentAsync(request);
            return Ok(result);
        }

        // Outstanding Report
        [HttpGet("outstanding")]
        public async Task<IActionResult> GetOutstandingInvoices()
        {
            var result = await arInvoiceService.GetOutstandingInvoicesAsync();
            return Ok(result);
        }

        // Aging Report
        [HttpGet("aging")]
        public async Task<IActionResult> GetAgingReport()
        {
            var result = await arInvoiceService.GetAgingReportAsync();
            return Ok(result);
        }
    }
}