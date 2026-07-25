using Fincore.Application.DTO.Payment.AccountsReceivable.Requests;
using Fincore.Application.DTO.Payment.APInvoice.Requests;
using Fincore.Application.Interfaces.Payment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.V1.Payment
{
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    [Route("api/v1/ap")]
    public class APInvoiceController : ControllerBase
    {
        private readonly IAPInvoiceService service;

        public APInvoiceController(IAPInvoiceService service)
        {
            this.service = service;
        }

        // POST : api/v1/ap/invoices
        [HttpPost("invoices")]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateAPInvoiceRequestDto request)
        {
            var result = await service.CreateAsync(request);

            if (!result.success)
                return BadRequest(result);

            return Ok(result);
        }

        // GET : api/v1/ap/invoices
        [HttpGet("invoices")]
        public async Task<IActionResult> GetAllInvoices([FromQuery] APInvoiceFilterDto filter)
        {
            var result = await service.GetAllAsync(filter);

            if (!result.success)
                return BadRequest(result);

            return Ok(result);
        }

        // PUT : api/v1/ap/invoices/{id}
        [HttpPut("invoices/{id:int}")]
        public async Task<IActionResult> UpdateInvoice(
            int id,
            [FromBody] UpdateAPInvoiceRequestDto request)
        {
            var result = await service.UpdateAsync(id, request);

            if (!result.success)
                return BadRequest(result);

            return Ok(result);
        }


        // DELETE : api/v1/ap/invoices/{id}
        [HttpDelete("invoices/{id:int}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var result = await service.DeleteAsync(id);

            if (!result.success)
                return BadRequest(result);

            return Ok(result);
        }



        // POST : api/v1/ap/invoices/{id}/approve
        [HttpPost("invoices/{id}/approve")]
        public async Task<IActionResult> ApproveInvoice(int id)
        {
            var result = await service.ApproveAsync(id);

            if (!result.success)
                return BadRequest(result);

            return Ok(result);
        }

        // POST : api/v1/ap/payments
        [HttpPost("payments")]
        public async Task<IActionResult> RecordPayment([FromBody] CreatePaymentRequestDto request)
        {
            var result = await service.RecordPaymentAsync(request);

            if (!result.success)
                return BadRequest(result);

            return Ok(result);
        }

        // GET : api/v1/ap/outstanding
        [HttpGet("outstanding")]
        public async Task<IActionResult> GetOutstanding([FromQuery] APOutstandingFilterDto filter)
        {
            var result = await service.GetOutstandingAsync(filter);

            if (!result.success)
                return BadRequest(result);

            return Ok(result);
        }

        // GET : api/v1/ap/aging
        [HttpGet("aging")]
        public async Task<IActionResult> GetAgingReport([FromQuery] APAgingFilterDto filter)
        {
            var result = await service.GetAgingReportAsync(filter);

            if (!result.success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}