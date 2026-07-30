using AutoMapper;
using Fincore.Application.DTO.Payment.APInvoice.Requests;
using Fincore.Application.DTO.Payment.APInvoice.Responses;
using Fincore.Domain.Models;

namespace Fincore.Application.Mapper
{
    public class APInvoiceProfile : Profile
    {
        public APInvoiceProfile()
        {
            CreateMap<CreateAPInvoiceRequestDto, APInvoice>();

            CreateMap<UpdateAPInvoiceRequestDto, APInvoice>();

            CreateMap<APInvoice, APInvoiceResponseDto>()
                .ForMember(dest => dest.VendorName,
                    opt => opt.MapFrom(src => src.Vendor.Company.CompanyName))

                .ForMember(dest => dest.PurchaseOrderNumber,
                    opt => opt.MapFrom(src => src.PurchaseOrder.POCode))

                .ForMember(dest => dest.GRNNumber,
                    opt => opt.MapFrom(src => src.GRN.GRNCode))

                .ForMember(dest => dest.WorkOrderNumber,
                    opt => opt.MapFrom(src => src.WorkOrder != null
                        ? src.WorkOrder.WONumber
                        : null))

                .ForMember(dest => dest.ApprovalStatus,
                    opt => opt.MapFrom(src => src.ApprovalStatus))

                .ForMember(dest => dest.PaymentStatus,
                    opt => opt.MapFrom(src => src.PaymentStatus));
        }
    }
}