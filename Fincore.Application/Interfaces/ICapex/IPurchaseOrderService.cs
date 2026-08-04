using Fincore.Application.DTO;
using Fincore.Application.DTO.Capex;
using Fincore.Application.DTO.Capex.PurchaseOrder;

namespace Fincore.Application.Interfaces.ICapex
{
    public interface IPurchaseOrderService
    {

        Task Create(PMCreateDTO dto);
        Task Update(PMUpdateDTO dto);
        Task Delete(int id);
        Task <PMItemDTO> ReadById(int id);
        Task <List<PMItemDTO>> ReadAll();


        Task DropDownQuotation();

    }
}