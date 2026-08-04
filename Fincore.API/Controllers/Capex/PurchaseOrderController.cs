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

        







    }
}