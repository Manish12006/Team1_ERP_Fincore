using AutoMapper;
using Fincore.Application.DTO.Capex;
using Fincore.Application.DTO.Capex.PurchaseOrder;
using Fincore.Domain.Models;


namespace Fincore.Infrastructure.Mapper
{
    public class PurchaseOrderMapperProfile : Profile
    {

        public PurchaseOrderMapperProfile()
        {

            CreateMap<PurchaseOrder, PMCreateDTO>().ReverseMap();
            CreateMap<PurchaseOrder, PMUpdateDTO>().ReverseMap();
            CreateMap<PurchaseOrder, PMItemDTO>().ReverseMap();



        }
    }
}