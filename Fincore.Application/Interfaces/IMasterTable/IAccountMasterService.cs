using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;
using Fincore.Domain.Enums;

namespace Fincore.Application.Interfaces.IMasterTable
{
    public interface IAccountMasterService
    {
        Task<ApiResponse<AccountMasterPostDTO>> AddAccountsMaster(AccountMasterPostDTO dto, int count, AccountType type);

        Task<int> GetCount();

        Task<ApiResponse<List<AccountMasterGetDTO>>> GetAllAccounts(int page, int pageSize);

        Task<ApiResponse<bool>> DeleteAccount(int id);

        Task<ApiResponse<AccountMasterGetDTO>> GetAccountById(int id);

        Task<ApiResponse<AccountMasterGetDTO>> UpdateAccount(int id, AccountMasterPutDTO dto, AccountType type);

        Task<ApiResponse<List<AccountMasterGetDTO>>> GetActiveAccounts(int page, int pageSize);

        Task<ApiResponse<List<AccountMasterGetDTO>>> GetPendingAccounts(int page, int pageSize);

        Task<ApiResponse<List<AccountMasterGetDTO>>> GetAccountType(AccountType type, int page, int pageSize);
    }
}
