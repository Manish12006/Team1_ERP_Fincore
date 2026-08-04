using Fincore.Application.DTO;
using Fincore.Application.DTO.Capex;
using Fincore.Application.DTO.Capex.GRN;

namespace Fincore.Application.Interfaces.ICapex
{
    public interface IGRNService
    {
        Task Create(GRNCreateDTO dto);
        Task Update(GRNUpdateDTO dto);
        Task Delete(int id);
        Task <GRNItemDTO>ReadById(int id);
        Task <List<GRNItemDTO>> ReadAll(); 
    }
}