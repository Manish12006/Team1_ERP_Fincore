using Fincore.Application.DTOs.WorkOrder;
using Fincore.Application.Interfaces.WorkOrder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers
{
    [ApiController]
    [Route("api/v1/[action]")]
    [EnableRateLimiting("FixedPolicy")]
    public class WorkOrderController : ControllerBase
    {
        private readonly IWorkOrderService _workOrderService;

        public WorkOrderController(IWorkOrderService workOrderService)
        {
            _workOrderService = workOrderService;
        }

        // Create
        [HttpPost]
        public async Task<IActionResult> AddWorkOrder(CreateWorkOrderDTO dto)
        {
            var response = await _workOrderService.AddWorkOrder(dto);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }

        // Get All
        [HttpGet]
        public async Task<IActionResult> GetWorkOrders(
       string? title,
       int? vendorId,
       int? opexRequestId,
       string? status,
       int page = 1,
       int pageSize = 5)


        {
            if (page <= 0)
            {
                return BadRequest("Page Number must be greater than 0");
            }

            if (pageSize <= 0)
            {
                return BadRequest("Page Size must be greater than 0");
            }

            if (pageSize > 50)
            {
                return BadRequest("Maximum Page Size is 50");
            }
            var response = await _workOrderService.GetWorkOrders(
                title,
                vendorId,
                opexRequestId,
                status,
                page,
                pageSize);

            return Ok(response);
        }

        // Get By Id
        [HttpGet]
        public async Task<IActionResult> GetWorkOrderById(int id)
        {
            var response = await _workOrderService.GetWorkOrderById(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }

        // Update
        [HttpPut]
        public async Task<IActionResult> UpdateWorkOrder(int id, UpdateWorkOrderDTO dto)
        {
            var response = await _workOrderService.UpdateWorkOrder(id, dto);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }

        // Delete
        [HttpDelete]
        public async Task<IActionResult> DeleteWorkOrder(int id)
        {
            var response = await _workOrderService.DeleteWorkOrder(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }

        // Approve
        [HttpPost]
        public async Task<IActionResult> ApproveWorkOrder(int id)
        {
            var response = await _workOrderService.ApproveWorkOrder(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }

        // Reject
        [HttpPost]
        public async Task<IActionResult> RejectWorkOrder(int id)
        {
            var response = await _workOrderService.RejectWorkOrder(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }

        // Summary
        [HttpGet]
        public async Task<IActionResult> GetWorkOrderSummary()
        {
            var response = await _workOrderService.GetWorkOrderSummary();
            return Ok(response);
        }
    }
}