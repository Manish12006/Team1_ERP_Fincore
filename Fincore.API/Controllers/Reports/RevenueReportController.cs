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
    public class RevenueReportController : ControllerBase
    {
        private readonly IRevenueReportService service;

        public RevenueReportController(IRevenueReportService service)
        {
            this.service = service;
        }

        // Get All Revenue Report
        [HttpGet]
        public async Task<IActionResult> GetRevenue(int page = 1, int pageSize = 10)
        {
            var response = await service.GetRevenueAsync(page, pageSize);
            return Ok(response);
        }

        // Customer Wise Revenue Report
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetRevenueByCustomer(int customerId,int page = 1,int pageSize = 10)
        {
            var response = await service.GetRevenueByCustomerAsync(customerId, page, pageSize);
            return Ok(response);
        }

        // Department Wise Revenue Report
        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> GetRevenueByDepartment(int departmentId, int page = 1, int pageSize = 10)
        {
            var response = await service.GetRevenueByDepartmentAsync(departmentId, page, pageSize);
            return Ok(response);
        }

        // Account Wise Revenue Report
        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetRevenueByAccount(int accountId,int page = 1, int pageSize = 10)
        {
            var response = await service.GetRevenueByAccountAsync(accountId, page, pageSize);
            return Ok(response);
        }

        // Revenue Type Wise Report
        [HttpGet("type/{revenueType}")]
        public async Task<IActionResult> GetRevenueByRevenueType(RevenueType revenueType,int page = 1,int pageSize = 10)
        {
            var response = await service.GetRevenueByRevenueTypeAsync(revenueType, page, pageSize);
            return Ok(response);
        }

        // Revenue Status Wise Report
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetRevenueByStatus(RevenueStatus status,int page = 1,int pageSize = 10)
        {
            var response = await service.GetRevenueByStatusAsync(status, page, pageSize);
            return Ok(response);
        }

        // Revenue Report By Date Range
        [HttpGet("date-range")]
        public async Task<IActionResult> GetRevenueByDateRange(DateTime fromDate,DateTime toDate,int page = 1,int pageSize = 10)
        {
            var response = await service.GetRevenueByDateRangeAsync(fromDate, toDate, page, pageSize);
            return Ok(response);
        }
    }
}
