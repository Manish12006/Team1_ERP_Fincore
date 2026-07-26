using Fincore.Application.DTO;
using Fincore.Application.DTOs.OpexRequest;
using Fincore.Application.DTOs.Common;

namespace Fincore.Application.Interfaces.Opex
{
    public interface IOpexRequestService
    {
        Task<string> AddOpexRequest(CreateOpexRequestDTO dto);

        Task<List<OpexRequestResponseDTO>> GetOpexRequests(
      string? title,
      int? budgetLineId,
      int? requestedBy,
      string? approvalStatus,
      int page,
      int pageSize);

        Task<OpexRequestResponseDTO?> GetOpexRequestById(int id);

        Task UpdateOpexRequest(int id, UpdateOpexRequestDTO dto);

        Task DeleteOpexRequest(int id);

        Task<string> ApproveOpexRequest(int id, int approvedBy);

        Task<string> RejectOpexRequest(int id, int approvedBy);
        Task<OpexSummaryDTO> GetOpexSummary();
        Task<List<OpexDropDownDTO>> GetBudgetLineDropdown();

        Task<List<OpexDropDownDTO>> GetUserDropdown();
    }
}