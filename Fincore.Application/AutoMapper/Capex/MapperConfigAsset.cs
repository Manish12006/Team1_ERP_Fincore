using AutoMapper;
using Fincore.Application.DTO.Capex;
using Fincore.Domain.Models;

namespace Fincore.Application.AutoMapper.Capex
{
    public class MapperConfigAsset : Profile
    {
        public MapperConfigAsset()
        {
            CreateMap<Asset, AssetDTO>()
                .ReverseMap()
                .ForMember(
                    x => x.AssetId,
                    y => y.Ignore()
                );
        }
    }
}