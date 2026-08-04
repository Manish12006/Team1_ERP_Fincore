using Fincore.API.CommonHelper;
using Fincore.Application.DTO.Capex;
using Fincore.Application.DTO.Capex.PurchaseOrder;
using Fincore.Application.Interfaces.ICapex;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;


namespace Fincore.API.Controllers.Capex
{

    [Route("api/v1/[controller]")]
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    public class PurchaseOrderController : ControllerBase
    {

        private readonly IPurchaseOrderService service;

        public PurchaseOrderController(IPurchaseOrderService service)
        {
            this.service = service;
        }


        [HttpPost]
        public async Task<IActionResult> Create(PMCreateDTO dto)
        {
            await service.Create(dto);
            return Ok(ApiResponseHelper.SuccessRes<string>(null, "Data Created"));

        }

        [HttpPut]
        public async Task<IActionResult> Update(PMUpdateDTO dto)
        {
            await service.Update(dto);
            return Ok(ApiResponseHelper.SuccessRes<string>(null, "Data Upadated"));

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
           var data=  await service.ReadById(id);
            return Ok(ApiResponseHelper.SuccessRes<PMItemDTO>(data, "Data Fetched"));

        }


        [HttpGet]
        public async Task<IActionResult> ReadAll()
        {
            var data= await service.ReadAll();
            return Ok(ApiResponseHelper.SuccessRes<List<PMItemDTO>>(data, "Data Fetched"));

        }



        //[HttpPost]
        //public async Task<IActionResult> Dropdown(PMCreateDTO dto)
        //{
        //    //await service.Create(dto);
        //    return Ok(ApiResponseHelper.SuccessRes<string>(null, "Dropdown Fetched"));

        //}





    }
}