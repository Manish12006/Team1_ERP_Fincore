using Fincore.Application.DTO.Capex;
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
        public async Task<IActionResult> CreatePurchaseOrderItem(PurchaseOrderItemDTO dto)
        {
            return Ok(await service.AddPurchaseOrderItem(dto));
        }



        [HttpGet]
        public async Task<IActionResult> GetAllPurchaseOrderItems(
            int page = 1,
            int pageSize = 10)
        {
            return Ok(await service.GetAllPurchaseOrderItems(page, pageSize));
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetPurchaseOrderItem(int id)
        {
            return Ok(await service.GetPurchaseOrderItem(id));
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePurchaseOrderItem(
            int id,
            PurchaseOrderItemDTO dto)
        {
            return Ok(await service.UpdatePurchaseOrderItem(id, dto));
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurchaseOrderItem(int id)
        {
            return Ok(await service.DeletePurchaseOrderItem(id));
        }

        [HttpGet("by-po/{poId}")]
        public async Task<IActionResult> GetItemsByPOId(int poId)
        {
            return Ok(
                await service.GetItemsByPOId(poId)
            );
        }

    }
}