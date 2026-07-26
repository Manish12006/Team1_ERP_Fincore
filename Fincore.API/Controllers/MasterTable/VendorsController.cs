using Fincore.Application.DTO.MasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.MasterTable
{
    [Route("api/v1/vendors")]
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    public class VendorsController : ControllerBase
    {
        private readonly IVendorService vendorService;

        public VendorsController(IVendorService vendorService)
        {
            this.vendorService = vendorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVendors(
            int pageNumber = 1,
            int pageSize = 10,
            string? search=null)
        {
            var response = await vendorService
                .GetAllVendorsAsync(pageNumber, pageSize,search);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVendorById(int id)
        {
            var response = await vendorService
                .GetVendorByIdAsync(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVendor(
            [FromBody] CreateVendorDto createVendorDto)
        {
            var response = await vendorService
                .CreateVendorAsync(createVendorDto);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVendor(
            int id,
            [FromBody] UpdateVendorDto updateVendorDto)
        {
            var response = await vendorService
                .UpdateVendorAsync(id, updateVendorDto);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVendor(int id)
        {
            var response = await vendorService
                .DeleteVendorAsync(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }
    }
}