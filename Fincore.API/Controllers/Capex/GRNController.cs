using Fincore.Application.DTO.Capex;
using Fincore.Application.Interfaces.ICapex;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.Capex
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    public class GRNController : ControllerBase
    {
        private readonly IGRNService service;

        public GRNController(IGRNService service)
        {
            this.service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGRN([FromBody] GRNDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(await service.CreateGRN(dto));
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGRN(int id,[FromBody] GRNDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(await service.UpdateGRN(dto, id));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ReadGRNById(int id)
        {
            return Ok(await service.GetGRNById(id));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGRN(int id)
        {
            return Ok(await service.DeleteGRN(id));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGRN(int page=1,int pagesize = 10)
        {
            return Ok(await service.GetAllGRN(page, pagesize));
        }

        [HttpPut("approve-quality/{id}")]
        public async Task<IActionResult> ApproveQuality(int id)
        {
            return Ok(
                await service.ApproveQualityCheck(id)
            );
        }



        [HttpPut("reject-quality/{id}")]
        public async Task<IActionResult> RejectQuality(int id)
        {
            return Ok(
                await service.RejectQualityCheck(id)
            );
        }



        [HttpPut("close/{id}")]
        public async Task<IActionResult> CloseGRN(int id)
        {
            return Ok(
                await service.CloseGRN(id)
            );
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            return Ok(
                await service.GetGRNByStatus(status)
            );
        }

        [HttpGet("vendor/{vendorId}")]
        public async Task<IActionResult> GetByVendor(int vendorId)
        {
            return Ok(
                await service.GetGRNByVendor(vendorId)
            );
        }

        [HttpGet("purchase-order/{poId}")]
        public async Task<IActionResult> GetByPO(int poId)
        {
            return Ok(
                await service.GetGRNByPurchaseOrder(poId)
            );
        }

        [HttpPost("receive")]
        public async Task<IActionResult> ReceiveGoods(
        [FromBody] GRNDTO dto)
        {
            return Ok(
                await service.ReceiveGoods(dto)
            );
        }



        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetHistory(int id)
        {
            return Ok(
                await service.GetGRNHistory(id)
            );
        }

        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown()
        {
            return Ok(
            await service.GetGRNDropdown()
            );
        }
    }
}
