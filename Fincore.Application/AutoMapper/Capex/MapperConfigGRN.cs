using AutoMapper;
using Fincore.Application.DTO.Capex;
using Fincore.Domain.Models;

namespace Fincore.Application.AutoMapper.Capex
{
    public class MapperConfigGRN : Profile
    {
        public MapperConfigGRN()
        {
            CreateMap<GRN, GRNDTO>()
                .ReverseMap()
                .ForMember(x => x.GRNId,
                opt => opt.Ignore());
        }
    }
}