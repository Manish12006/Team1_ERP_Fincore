using Fincore.Application.DTO.Capex;
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
        public async Task<IActionResult> CreateAsset(
            [FromBody] AssetDTO dto)
        {
            return Ok(
                await service.AddAsset(dto)
            );
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAssets(
            int page = 1,
            int pageSize = 10)
        {
            return Ok(
                await service.GetAllAssets(page, pageSize)
            );
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsset(int id)
        {
            return Ok(
                await service.GetAsset(id)
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsset(
            int id,
            [FromBody] AssetDTO dto)
        {
            return Ok(
                await service.UpdateAsset(id, dto)
            );
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            return Ok(
                await service.DeleteAsset(id)
            );
        }


        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignAsset(
            int id,
            int userId)
        {
            return Ok(
                await service.AssignAsset(id, userId)
            );
        }

        [HttpPut("{id}/transfer")]
        public async Task<IActionResult> TransferAsset(
            int id,
            int departmentId)
        {
            return Ok(
                await service.TransferAsset(id, departmentId)
            );
        }

        [HttpPut("{id}/dispose")]
        public async Task<IActionResult> DisposeAsset(int id)
        {
            return Ok(
                await service.DisposeAsset(id)
            );
        }

        [HttpPut("{id}/repair")]
        public async Task<IActionResult> RepairAsset(int id)
        {
            return Ok(
                await service.RepairAsset(id)
            );
        }
        [HttpPut("{id}/return")]
        public async Task<IActionResult> ReturnAsset(int id)
        {
            return Ok(
                await service.ReturnAsset(id)
            );
        }


        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(
            string status)
        {
            return Ok(
                await service.GetAssetByStatus(status)
            );
        }

        [HttpGet("dropdown/vendors")]
        public async Task<IActionResult> GetVendorDropdown()
        {
            return Ok(await service.GetVendorDropdown());
        }

        [HttpGet("dropdown/departments")]
        public async Task<IActionResult> GetDepartmentDropdown()
        {
            return Ok(await service.GetDepartmentDropdown());
        }

        [HttpGet("dropdown/grn")]
        public async Task<IActionResult> GetGRNDropdown()
        {
            return Ok(await service.GetGRNDropdown());
        }

        [HttpGet("dropdown/purchase-orders")]
        public async Task<IActionResult> GetPurchaseOrderDropdown()
        {
            return Ok(await service.GetPurchaseOrderDropdown());
        }
    }
}