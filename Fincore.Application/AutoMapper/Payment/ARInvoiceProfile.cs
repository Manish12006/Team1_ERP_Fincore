using AutoMapper;
using Fincore.Application.DTO.Payment.AccountsReceivable.Requests;
using Fincore.Application.DTO.Payment.AccountsReceivable.Responses;
using Fincore.Application.DTO.Payment.APInvoice.Requests;
using Fincore.Application.DTO.Payment.APInvoice.Responses;
using Fincore.Domain.Models;

namespace Fincore.Infrastructure.Mapping.Payment
{
    public class ARInvoiceProfile : Profile
    {
        public ARInvoiceProfile()
        {
            // Create Invoice
            CreateMap<CreateARInvoiceRequestDto, ARInvoice>();

            // Update Invoice
            CreateMap<UpdateARInvoiceRequestDto, ARInvoice>();

            // Entity -> Response
            CreateMap<ARInvoice, ARInvoiceResponseDto>()
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer.Company.CompanyName))

                .ForMember(dest => dest.RevenueInvoiceNumber,
                    opt => opt.MapFrom(src => src.RevenueEntry.InvoiceNumber))

                .ForMember(dest => dest.AmountReceived,
                    opt => opt.MapFrom(src => src.AmountReceived))

                .ForMember(dest => dest.AmountOutstanding,
                    opt => opt.MapFrom(src => src.AmountOutstanding))

                .ForMember(dest => dest.PaymentStatus,
                    opt => opt.MapFrom(src => src.PaymentStatus));

            // Outstanding Report
            CreateMap<ARInvoice, AROutstandingResponseDto>()
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer.Company.CompanyName));

            // Payment Response
            CreateMap<ARInvoice, ARPaymentResponseDto>()
                .ForMember(dest => dest.PaidAmount,
                    opt => opt.MapFrom(src => src.AmountReceived))

                .ForMember(dest => dest.OutstandingAmount,
                    opt => opt.MapFrom(src => src.AmountOutstanding))

                .ForMember(dest => dest.PaymentDate,
                    opt => opt.Ignore());
        }
    }
}