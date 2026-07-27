using Fincore.Application.DTOs.OpexRequest;
using Fincore.Application.Interfaces.Opex;
using Fincore.API.CommonHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

//namespace Fincore.API.Controllers
//{
//    [ApiController]
//    [Route("api/v1/[action]")]
//    [EnableRateLimiting("FixedPolicy")]
//    public class OpexRequestV1Controller : ControllerBase
//    {
//        private readonly IOpexRequestService _opexService;

//        public OpexRequestV1Controller(IOpexRequestService opexService)
//        {
//            _opexService = opexService;
//        }

        // Create
        [HttpPost]
        public async Task<IActionResult> AddOpexRequest(CreateOpexRequestDTO dto)
        {
            var result = await _opexService.AddOpexRequest(dto);

            if (result != "Success")
            {
                return BadRequest(
                    ApiResponseHelper.Failure<object>(
                        "Validation Failed",
                        "BAD REQUEST",
                        result));
            }

            return Ok(
                ApiResponseHelper.SuccessRes(
                    result,
                    "Opex Request Added Successfully",
                    1));
        }

        // Get All
        [HttpGet]
        public async Task<IActionResult> GetOpexRequests(
    string? title,
    int? budgetLineId,
    int? requestedBy,
    string? approvalStatus,
    int page = 1,
    int pageSize = 5)
        {
            // Page Validation
            if (page <= 0)
            {
                return BadRequest(
                    ApiResponseHelper.Failure<object>(
                        "Invalid Page Number",
                        "BAD_REQUEST",
                        "Page number must be greater than 0"));
            }

            // Page Size Validation
            if (pageSize <= 0)
            {
                return BadRequest(
                    ApiResponseHelper.Failure<object>(
                        "Invalid Page Size",
                        "BAD_REQUEST",
                        "Page size must be greater than 0"));
            }

            // Maximum Page Size Validation
            if (pageSize > 50)
            {
                return BadRequest(
                    ApiResponseHelper.Failure<object>(
                        "Invalid Page Size",
                        "BAD_REQUEST",
                        "Maximum page size allowed is 50"));
            }

            var data = await _opexService.GetOpexRequests(
                title,
                budgetLineId,
                requestedBy,
                approvalStatus,
                page,
                pageSize);

//            var response = ApiResponseHelper.SuccessRes(
//                data,
//                "Opex Requests Fetched Successfully",
//                data.Count);

//            return Ok(response);
//        }

//        // Get By Id
//        [HttpGet]
//        public async Task<IActionResult> GetOpexRequestById(int id)
//        {
//            var data = await _opexService.GetOpexRequestById(id);

//            if (data == null)
//            {
//                return NotFound(
//                    ApiResponseHelper.Failure<object>(
//                        "Record Not Found",
//                        "NOT_FOUND",
//                        $"Opex Request not found with id : {id}"
//                    ));
//            }

//            var response = ApiResponseHelper.SuccessRes(
//                data,
//                "Opex Request Found Successfully",
//                1);

//            return Ok(response);
//        }

//        // Update
//        [HttpPut]
//        public async Task<IActionResult> UpdateOpexRequest(int id, UpdateOpexRequestDTO dto)
//        {
//            var data = await _opexService.GetOpexRequestById(id);

//            if (data == null)
//            {
//                return NotFound(
//                    ApiResponseHelper.Failure<object>(
//                        "Record Not Found",
//                        "NOT_FOUND",
//                        $"Opex Request not found with id : {id}"
//                    ));
//            }

//            await _opexService.UpdateOpexRequest(id, dto);

//            return Ok(new
//            {
//                message = "Opex Request Updated Successfully"
//            });
//        }

        // Soft Delete
        [HttpDelete]
        public async Task<IActionResult> DeleteOpexRequest(int id)
        {
            var data = await _opexService.GetOpexRequestById(id);

//            if (data == null)
//            {
//                return NotFound(
//                    ApiResponseHelper.Failure<object>(
//                        "Record Not Found",
//                        "NOT_FOUND",
//                        $"Opex Request not found with id : {id}"
//                    ));
//            }

//            await _opexService.DeleteOpexRequest(id);

//            return Ok(new
//            {
//                message = "Opex Request Deleted Successfully"
//            });
//        }

        // Approve
        [HttpPost]
        public async Task<IActionResult> ApproveOpexRequest(int id, int approvedBy)
        {
            var result = await _opexService.ApproveOpexRequest(id, approvedBy);

            if (result != "Success")
            {
                return BadRequest(
                    ApiResponseHelper.Failure<object>(
                        "Operation Failed",
                        "BAD_REQUEST",
                        result));
            }

            return Ok(
                ApiResponseHelper.SuccessRes(
                    result,
                    "Opex Request Approved Successfully",
                    1));
        }

//        // Reject
//        [HttpPost]
//        public async Task<IActionResult> RejectOpexRequest(int id, int approvedBy)
//        {
//            var result = await _opexService.RejectOpexRequest(id, approvedBy);

            if (result != "Success")
            {
                return BadRequest(
                    ApiResponseHelper.Failure<object>(
                        "Operation Failed",
                        "BAD_REQUEST",
                        result));
            }

            return Ok(
                ApiResponseHelper.SuccessRes(
                    result,
                    "Opex Request Rejected Successfully",
                    1));
        }

        // Summary
        [HttpGet]
        public async Task<IActionResult> GetOpexSummary()
        {
            var summary = await _opexService.GetOpexSummary();

            return Ok(ApiResponseHelper.SuccessRes(
                summary,
                "Summary Fetched Successfully",
                1));
        }
        [HttpGet]
        public async Task<IActionResult> GetBudgetLineDropdown()
        {
            var data = await _opexService.GetBudgetLineDropdown();

            return Ok(ApiResponseHelper.SuccessRes(
                data,
                "Budget Line Dropdown Fetched Successfully",
                data.Count));
        }
        [HttpGet]
        public async Task<IActionResult> GetUserDropdown()
        {
            var data = await _opexService.GetUserDropdown();

            return Ok(ApiResponseHelper.SuccessRes(
                data,
                "User Dropdown Fetched Successfully",
                data.Count));
        }
    }
}