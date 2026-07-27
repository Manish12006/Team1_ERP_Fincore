
//using Fincore.Application.DTOs.OpexRequest;
//using Fincore.Application.Interfaces.Opex;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.RateLimiting;

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

//        // Create
//        [HttpPost]
//        public async Task<IActionResult> AddOpexRequest(CreateOpexRequestDTO dto)
//        {
//            await _opexService.AddOpexRequest(dto);

//            return Ok(new
//            {
//                message = "Opex Request Added Successfully"
//            });
//        }

//        // Get All
//        [HttpGet]
//        public async Task<IActionResult> GetOpexRequests(int page = 1, int pageSize = 5)
//        {
//            var data = await _opexService.GetOpexRequests(page, pageSize);

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

//        // Delete
//        [HttpDelete]
//        public async Task<IActionResult> DeleteOpexRequest(int id)
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

//            await _opexService.DeleteOpexRequest(id);

//            return Ok(new
//            {
//                message = "Opex Request Deleted Successfully"
//            });
//        }

//        [HttpPost]
//        public async Task<IActionResult> ApproveOpexRequest(int id, int approvedBy)
//        {
//            var result = await _opexService.ApproveOpexRequest(id, approvedBy);

//            if (result == "Opex Request Not Found")
//            {
//                return NotFound(ApiResponseHelper.Failure<object>(
//                    "Record Not Found",
//                    "NOT_FOUND",
//                    result));
//            }

//            return Ok(ApiResponseHelper.SuccessRes(
//                result,
//                result,
//                1));
//        }

//        // Reject
//        [HttpPost]
//        public async Task<IActionResult> RejectOpexRequest(int id, int approvedBy)
//        {
//            var result = await _opexService.RejectOpexRequest(id, approvedBy);

//            if (result == "Opex Request Not Found")
//            {
//                return NotFound(ApiResponseHelper.Failure<object>(
//                    "Record Not Found",
//                    "NOT_FOUND",
//                    result));
//            }

//            return Ok(ApiResponseHelper.SuccessRes(
//                result,
//                result,
//                1));
//        }
//        [HttpGet]
//        public async Task<IActionResult> GetOpexSummary()
//        {
//            var summary = await _opexService.GetOpexSummary();

//            return Ok(ApiResponseHelper.SuccessRes(
//                summary,
//                "Summary Fetched Successfully",
//                1));
//        }
//    }
//}