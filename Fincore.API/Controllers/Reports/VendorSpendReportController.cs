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
    public class VendorSpendReportController : ControllerBase
    {
        private readonly IVendorSpendReportService service;

        public VendorSpendReportController(IVendorSpendReportService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetVendorSpend(
            int page = 1,
            int pageSize = 10)
        {
            var result = await service.GetVendorSpendAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("vendor/{vendorId}")]
        public async Task<IActionResult> GetVendorSpendByVendor(
            int vendorId,
            int page = 1,
            int pageSize = 10)
        {
            var result = await service.GetVendorSpendByVendorAsync(vendorId, page, pageSize);
            return Ok(result);
        }

        [HttpGet("payment-status/{paymentStatus}")]
        public async Task<IActionResult> GetVendorSpendByPaymentStatus(
            PaymentStatus paymentStatus,
            int page = 1,
            int pageSize = 10)
        {
            var result = await service.GetVendorSpendByPaymentStatusAsync(paymentStatus, page, pageSize);
            return Ok(result);
        }

        [HttpGet("approval-status/{approvalStatus}")]
        public async Task<IActionResult> GetVendorSpendByApprovalStatus(
            ApprovalStatus approvalStatus,
            int page = 1,
            int pageSize = 10)
        {
            var result = await service.GetVendorSpendByApprovalStatusAsync(approvalStatus, page, pageSize);
            return Ok(result);
        }

        [HttpGet("date-range")]
        public async Task<IActionResult> GetVendorSpendByDateRange(
            DateTime fromDate,
            DateTime toDate,
            int page = 1,
            int pageSize = 10)
        {
            var result = await service.GetVendorSpendByDateRangeAsync(fromDate, toDate, page, pageSize);
            return Ok(result);
        }
    }
}
