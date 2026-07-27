using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fincore.Application.CommonHelper;
using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;
using Fincore.Domain.Models;

namespace Fincore.Application.Interfaces.IMasterTable
{
    public interface ICurrencyService
    {
        Task<ApiResponse<PagedResponse<CurrencyResponseDto>>> GetAllAsync(
            int page,
            int pageSize,
            string? currencyName = null);

        Task<ApiResponse<CurrencyResponseDto>> GetByIdAsync(int id);

        Task<ApiResponse<CurrencyResponseDto>> CreateAsync(CurrencyDto dto);

        Task<ApiResponse<CurrencyResponseDto>> UpdateAsync(
            int id,
            CurrencyDto dto);

        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
