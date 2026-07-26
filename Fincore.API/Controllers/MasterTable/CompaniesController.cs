using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Microsoft.AspNetCore.Mvc;

namespace Fincore.API.Controllers.MasterTable
{
    [ApiController]
    [Route("api/v1/companies")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService companyService;

        
        public CompaniesController(ICompanyService companyService)
        {
            this.companyService = companyService;
        }

        private IActionResult GetResponse<T>(ApiResponse<T> result)
        {
            if (result.success)
            {
                return Ok(result);
            }

            return result.error?.Code switch
            {
                "404" => NotFound(result),
                "409" => Conflict(result),
                _ => BadRequest(result)
            };
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany(CreateCompanyDto dto)
        {
            var result = await companyService.CreateCompanyAsync(dto);

            return GetResponse(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCompanies(int page = 1, int limit = 10, string? search = null)
        {
            var result = await companyService.GetAllCompaniesAsync(page, limit,search);

            return GetResponse(result);
        }

        [HttpGet("{companyId}")]
        public async Task<IActionResult> GetCompanyById(int companyId)
        {
            var result = await companyService.GetCompanyByIdAsync(companyId);

            return GetResponse(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCompany(UpdateCompanyDto dto)
        {
            var result = await companyService.UpdateCompanyAsync(dto);

            return GetResponse(result);
        }

        [HttpDelete("{companyId}")]
        public async Task<IActionResult> DeleteCompany(int companyId)
        {
            var result = await companyService.DeleteCompanyAsync(companyId);

            return GetResponse(result);
        }
    }
}