using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.Interfaces.IMasterTable
{
    public interface ICompanyService
    {
        Task<ApiResponse<CompanyDto>> CreateCompanyAsync(CreateCompanyDto dto);

        Task<ApiResponse<List<CompanyDto>>> GetAllCompaniesAsync(int page, int limit, string? search);

        Task<ApiResponse<CompanyDto>> GetCompanyByIdAsync(int companyId);

        Task<ApiResponse<CompanyDto>> UpdateCompanyAsync(UpdateCompanyDto dto);

        Task<ApiResponse<string>> DeleteCompanyAsync(int companyId);
    }
}
