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



        public PurchaseOrderController(
            IPurchaseOrderService service)
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





        // Workflow


        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(
            int id)
        {
            return Ok(
                await service.ApprovePurchaseOrder(id));
        }





        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(
            int id)
        {
            return Ok(
                await service.CancelPurchaseOrder(id));
        }




        [HttpPost("{id}/close")]
        public async Task<IActionResult> Close(
            int id)
        {
            return Ok(
                await service.ClosePurchaseOrder(id));
        }






        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(
            string status,
            int page = 1,
            int pageSize = 10)
        {
            return Ok(
                await service.GetPurchaseOrderByStatus(
                    status,
                    page,
                    pageSize));
        }






        [HttpGet("filter")]
        public async Task<IActionResult> Filter(
            [FromQuery] PurchaseOrderFilterDTO filter)
        {
            return Ok(
                await service.FilterPurchaseOrders(filter));
        }






        // Dropdown API

        [HttpGet("dropdowns")]
        public async Task<IActionResult> Dropdowns()
        {
            return Ok(
                await service.GetPurchaseOrderDropdowns());
        }





        // Pending Approval API

        [HttpGet("pending")]
        public async Task<IActionResult> Pending()
        {
            return Ok(
                await service.GetPendingPurchaseOrders());
        }





        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GeneratePdf(
            int id)
        {

            var pdf =
                await service.GeneratePurchaseOrderPdf(id);


            if (pdf == null)
            {
                return NotFound(
                    "Purchase Order Not Found");
            }


            return File(
                pdf,
                "application/pdf",
                $"PurchaseOrder_{id}.pdf");
        }

        [HttpGet("dropdown/vendors")]
        public async Task<IActionResult> VendorDropdown()
        {
            return Ok(await service.GetVendorDropdown());
        }



        [HttpGet("dropdown/purchase-requisitions")]
        public async Task<IActionResult> PurchaseRequisitionDropdown()
        {
            return Ok(await service.GetPurchaseRequisitionDropdown());
        }



        [HttpGet("dropdown/quotations")]
        public async Task<IActionResult> QuotationDropdown()
        {
            return Ok(await service.GetQuotationDropdown());
        }



        [HttpGet("dropdown/users")]
        public async Task<IActionResult> UserDropdown()
        {
            return Ok(await service.GetUserDropdown());
        }



        [HttpGet("dropdown/status")]
        public async Task<IActionResult> StatusDropdown()
        {       
            return Ok(await service.GetApprovalStatusDropdown());
        }
    }
}