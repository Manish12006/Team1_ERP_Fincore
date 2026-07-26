using Fincore.Application.DTO.MasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fincore.API.Controllers.MasterTable
{
    [Route("api/v1/customers")]
    [ApiController]
    [EnableRateLimiting("FixedPolicy")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService customerService;

        public CustomersController(ICustomerService customerService)
        {
            this.customerService = customerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCustomers(
            int pageNumber = 1,
            int pageSize = 10,
             string? search=null)
        {
            var response = await customerService
                .GetAllCustomersAsync(pageNumber, pageSize,search);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var response = await customerService
                .GetCustomerByIdAsync(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }


        [HttpPost]
        public async Task<IActionResult> CreateCustomer(
            [FromBody] CreateCustomerDto createCustomerDto)
        {
            var response = await customerService
                .CreateCustomerAsync(createCustomerDto);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(
            int id,
            [FromBody] UpdateCustomerDto updateCustomerDto)
        {
            var response = await customerService
                .UpdateCustomerAsync(id, updateCustomerDto);

            if (!response.success)
                return BadRequest(response);

            return Ok(response);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var response = await customerService
                .DeleteCustomerAsync(id);

            if (!response.success)
                return NotFound(response);

            return Ok(response);
        }
    }
}