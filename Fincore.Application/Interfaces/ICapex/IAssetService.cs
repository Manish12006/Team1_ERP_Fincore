using Fincore.Application.DTO;
using Fincore.Application.DTO.Capex;
using Fincore.Application.DTO.Capex.Assets;


namespace Fincore.Application.Interfaces.ICapex
{
    public interface IAssetService
    {
        Task Create(AssetsCreateDTO dto);
        Task Update(AssetsUpdateDTO dto);
        Task Delete(int id);
        Task <AssetsItemDTO>ReadById(int id);
        Task <List<AssetsItemDTO>>ReadAll();
        

    }
}