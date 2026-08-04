using Fincore.API.CommonHelper;
using Fincore.Application.DTO.Capex;
using Fincore.Application.DTO.Capex.PurchaseOrderItems;
using Fincore.Application.Interfaces.ICapex;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.Capex
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    public class PurchaseOrderItemController : ControllerBase
    {
        private readonly IPurchaseOrderItemService service;


        public PurchaseOrderItemController(IPurchaseOrderItemService service)
        {
            this.service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(POICreateDTO dto)
        {
            await service.Create(dto);
            return Ok(ApiResponseHelper.SuccessRes<string>(null, "Data Created"));
        }

        [HttpPut]
        public async Task<IActionResult> Update(POIUpdateDTO dto)
        {
            await service.Update(dto);
            return Ok(ApiResponseHelper.SuccessRes<string>(null, "Data Updated"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await service.Delete(id);
            return Ok(ApiResponseHelper.SuccessRes<string>(null, "Data Deleted"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ReadById(int id)
        {
            var data = await service.ReadById(id);
            return Ok(ApiResponseHelper.SuccessRes<POIItemDTO>(data, "Data Fetcned"));
        }


        [HttpGet]
        public async Task<IActionResult> ReadAll()
        {
            var data=await service.ReadAll();
            return Ok(ApiResponseHelper.SuccessRes<List<POIItemDTO>>(data, "Data Fetcned"));
        }


        //[HttpPost]
        //public async Task<IActionResult> Dropdown(POICreateDTO dto)
        //{
        //    //await service.Create(dto);
        //    return Ok(ApiResponseHelper.SuccessRes<string>(null, "Dropdown Fetched"));

        //}



    }
}