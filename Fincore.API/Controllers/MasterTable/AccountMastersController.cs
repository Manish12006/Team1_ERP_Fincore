using Fincore.Application.DTO;
using Fincore.Application.DTO.MasterTable;
using Fincore.Application.Interfaces.IMasterTable;
using Fincore.Domain.Enums;
using Fincore.Infrastructure.CommonHelper;
using Fincore.Infrastructure.Services.MasterTable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NPOI.SS.UserModel;


namespace Fincore.API.Controllers.MasterTable
{
    [Route("api/v1/accountmasters")]
    [ApiController]
    
    public class AccountMastersController : ControllerBase
    {
        private readonly IAccountMasterService _service;

        public AccountMastersController(IAccountMasterService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> AddAccountMasters(AccountMasterPostDTO dto, AccountType type)
        {
            try
            {
                var count = await _service.GetCount();
                
                var result = await _service.AddAccountsMaster(dto, count, type);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    success = false,
                    message = "Failed to create account.",
                    error = new ApiError
                    {
                        Code = "ACCOUNT_CREATION_FAILED",
                        Details = ex.Message
                    }
                });
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetAllAccounts(int page = 1, int pageSize = 10)
        {
            try
            {
                var result = await _service.GetAllAccounts(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(
                "Failed to fetch accounts.",
                "GET_ACCOUNT_FAILED",
                ex.Message
            ));

            }

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteById(int id)
        {
            try
            {
                var result = await _service.DeleteAccount(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<bool>(
                "Failed to delete account.",
                "DELETE_ACCOUNT_FAILED",
                ex.Message
            ));
            }

        }
        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> GetAccountById(int id)
        {
            try
            {
                var result = await _service.GetAccountById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<AccountMasterGetDTO>(
                "Failed to Find account.",
                "FIND_ACCOUNT_FAILED",
                ex.Message
            ));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, AccountMasterPutDTO dto, AccountType type)
        {
            try
            {
                var result = await _service.UpdateAccount(id, dto, type);

                if (!result.success)
                    return NotFound(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    ApiResponseHelper.Failure<AccountMasterGetDTO>(
                        "Failed to update account.",
                        "UPDATE_ACCOUNT_FAILED",
                        ex.Message
                    ));
            }
        }

        [HttpGet("Active")]
        public async Task<IActionResult> GetActiveAccounts(int page = 1, int pageSize=1)
        {
            try
            {
                var result = await _service.GetActiveAccounts(page, pageSize);
                return Ok(result);
            }catch(Exception ex)
            {
                return BadRequest(
            ApiResponseHelper.Failure<List<AccountMasterGetDTO>>(
                "Failed to fetch active accounts.",
                "GET_ACTIVE_ACCOUNTS_FAILED",
                ex.Message
            ));
            }
            

        }
        [HttpGet("Pending")]
        public async Task<IActionResult> GetPendingAccounts(int page =1, int pageSize = 1)
        {
            try
            {
                var result = await _service.GetPendingAccounts(page, pageSize);
                return Ok(result);

            }catch(Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<List<AccountMasterGetDTO>>(
                    "Failed to fetch pending accounts.",
                        "PENDING_ACCOUNTS_FAILED",
                        ex.Message
                    ));
            }

        }

        [HttpGet("AccountType/{type}")]
        public async Task<IActionResult> GetAccountsByType(AccountType type,int page = 1, int pageSize=1)
        {
            try
            {
                var result = await _service.GetAccountType(type, page, pageSize);
                return Ok(result);
            }catch(Exception e)
            {
                return BadRequest(ApiResponseHelper.Failure<List<AccountMasterGetDTO>>(
                    "Failed to fetch accounts.",
                        "ACCOUNTS_FETCH_FAILED",
                        e.Message
                    ));
            }
            

        }

    }
}
