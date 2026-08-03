using Fincore.Application.DTO.Capex;
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
        public async Task<IActionResult> CreatePurchaseOrder(
            PurchaseOrderDTO dto)
        {
            return Ok(
                await service.AddPurchaseOrder(dto));
        }


        [HttpGet]
        public async Task<IActionResult> GetAllPurchaseOrder(
            int page = 1,
            int pageSize = 10)
        {
            return Ok(
                await service.GetAllPurchaseOrder(
                    page,
                    pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPurchaseOrder(
            int id)
        {
            return Ok(
                await service.GetPurchaseOrder(id));
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePurchaseOrder(
            int id,
            PurchaseOrderDTO dto)
        {
            return Ok(
                await service.UpdatePurchaseOrder(
                    id,
                    dto));
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurchaseOrder(
            int id)
        {
            return Ok(
                await service.DeletePurchaseOrder(id));
        }

        [HttpGet("dropdown/quotations")]
        public async Task<IActionResult> QuotationDropdown()
        {
            return Ok(await service.GetQuotationDropdown());
        }



       



       
    }
}