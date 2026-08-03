using AutoMapper;
using Fincore.Application.DTO.Capex;
using Fincore.Domain.Models;


namespace Fincore.Infrastructure.Mapper
{
    public class PurchaseOrderMapperProfile : Profile
    {

        public PurchaseOrderMapperProfile()
        {


            CreateMap<PurchaseOrder, PurchaseOrderDTO>()

                .ForMember(
                    dest => dest.VendorCode,
                    opt => opt.MapFrom(
                        src => src.Vendor != null
                        ? src.Vendor.VendorCode
                        : null
                    )
                );



            CreateMap<PurchaseOrderDTO, PurchaseOrder>()

                .ForMember(
                    dest => dest.POId,
                    opt => opt.Ignore()
                )


                .ForMember(
                    dest => dest.POCode,
                    opt => opt.Ignore()
                )


                .ForMember(
                    dest => dest.Vendor,
                    opt => opt.Ignore()
                )


                .ForMember(
                    dest => dest.PurchaseRequisition,
                    opt => opt.Ignore()
                )


                .ForMember(
                    dest => dest.Quotation,
                    opt => opt.Ignore()
                )





                .ForMember(
                    dest => dest.CreatedByUser,
                    opt => opt.Ignore()
                )


                .ForMember(
                    dest => dest.ModifiedByUser,
                    opt => opt.Ignore()
                );


                

        }
    }
}