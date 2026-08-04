using Fincore.API.CommonHelper;
using Fincore.Application.DTO.Capex;
using Fincore.Application.DTO.Capex.Assets;
using Fincore.Application.DTO.Capex.PurchaseOrder;
using Fincore.Application.Interfaces.ICapex;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.Capex
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    public class AssetController : ControllerBase
    {
        private readonly IAssetService service;

        public AssetController(IAssetService service)
        {
            this.service = service;
        }
        [HttpPost]
        public async Task<IActionResult> Create(AssetsCreateDTO dto)
        {
            await service.Create(dto);
            return Ok(ApiResponseHelper.SuccessRes<string>(null, "Data Created"));

        }

        [HttpPut]
        public async Task<IActionResult> Update(AssetsUpdateDTO dto)
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
            var data = await service.ReadById(id);
            return Ok(ApiResponseHelper.SuccessRes<AssetsItemDTO>(data, "Data Fetched"));

        }


        [HttpGet]
        public async Task<IActionResult> ReadAll()
        {
            
            var data = await service.ReadAll();
            return Ok(ApiResponseHelper.SuccessRes<List<AssetsItemDTO>>(data, "Data Fetched"));

        }


    }
}