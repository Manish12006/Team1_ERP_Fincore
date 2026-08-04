using AutoMapper;
using Fincore.Application.DTO.Capex;
using Fincore.Application.DTO.Capex.PurchaseOrder;
using Fincore.Application.DTO.Capex.PurchaseOrderItems;
using Fincore.Domain.Models;


namespace Fincore.Infrastructure.Mapper
{
    public class PurchaseOrderMapperProfile : Profile
    {

        public PurchaseOrderMapperProfile()
        {

            //PURCHASE ORDER
            CreateMap<PurchaseOrder, PMCreateDTO>().ReverseMap();
            CreateMap<PurchaseOrder, PMUpdateDTO>().ReverseMap();
            CreateMap<PurchaseOrder, PMItemDTO>().ReverseMap();


            //PURCHSE ORDER ITEM
            CreateMap<PurchaseOrderItem, POICreateDTO>().ReverseMap();
            CreateMap<PurchaseOrderItem, POIUpdateDTO>().ReverseMap();
            CreateMap<PurchaseOrderItem, POIItemDTO>().ReverseMap();

        }
    }
}