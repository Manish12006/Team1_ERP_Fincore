using Fincore.Application.DTO;
using Fincore.Application.DTO.Capex;
using Fincore.Application.DTO.Capex.PurchaseOrderItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.Interfaces.ICapex
{
    public interface IPurchaseOrderItemService
    {
        Task Create(POICreateDTO dto);
        Task Update(POIUpdateDTO dto);
        Task Delete(int id);
        Task <POIItemDTO> ReadById(int id);
        Task<List<POIItemDTO>> ReadAll();

        Task DropDownQuotationId();
    }
}
