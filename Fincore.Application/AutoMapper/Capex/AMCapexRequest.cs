using AutoMapper;
using Fincore.Application.DTO.Capex;
using Fincore.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.AutoMapper.Capex
{
    public class AMCapexRequest : Profile
    {
        public AMCapexRequest() 
        {
            //Capex Request
            CreateMap<CapexRequest, CapexReqDTOPost>().ReverseMap();
            CreateMap<CapexReqDTOPost, CapexRequest>()
              .ForMember(dest => dest.CapexRequestId, opt => opt.Ignore());
            CreateMap<CapexRequest, CapexReqDTOGet>();

            //PR 
            CreateMap<PurchaseRequisition, PRDTOPost>().ReverseMap();
            CreateMap<PRDTOPost, PurchaseRequisition>()
             .ForMember(dest => dest.PurchaseRequisitionId, opt => opt.Ignore());
            CreateMap<PurchaseRequisition, PRDTOGet>()                
                .ForMember(x => x.RequestedByName, x => x.MapFrom(x => x.RequestedByUser.FullName != null ? x.RequestedByUser.FullName : "Not Available"))
                .ForMember(x => x.ApprovedByName, x => x.MapFrom(x => x.ApprovedByUser.FullName != null ? x.ApprovedByUser.FullName : "Not Available"))
                .ForMember(x => x.CreatedByName, x => x.MapFrom(x => x.CreatedByUser.FullName != null ? x.CreatedByUser.FullName : "Not Available"))
                .ForMember(x => x.ModifiedByName, x => x.MapFrom(x => x.ModifiedByUser.FullName != null ? x.ModifiedByUser.FullName : "Not Available"));

            //PRItem
            CreateMap<PurchaseRequisitionItem, PRItemDTOPost>().ReverseMap();
            CreateMap<PRItemDTOPost, PurchaseRequisitionItem>()
              .ForMember(dest => dest.PRItemId, opt => opt.Ignore());
            CreateMap<PurchaseRequisitionItem, PRItemDTOGet>()
                .ForMember(x => x.PRName, x => x.MapFrom(x => x.PurchaseRequisition.PRTitle != null ? x.PurchaseRequisition.PRTitle : "Not Available"))
                .ForMember(x => x.Category, x => x.MapFrom(x => x.VendorCategory.CategoryName != null ? x.VendorCategory.CategoryName : "Not Available"));

            //RFQ
            CreateMap<RFQ, RFQDTOPost>().ReverseMap();
            CreateMap<RFQDTOPost, RFQ>()
              .ForMember(dest => dest.RFQId, opt => opt.Ignore());
            CreateMap<RFQ, RFQDTOGet>()
                .ForMember(x => x.PRName, x => x.MapFrom(x => x.PurchaseRequisition.PRTitle != null ? x.PurchaseRequisition.PRTitle : "Not Available"))               
                .ForMember(x => x.CreatedByUser, x => x.MapFrom(x => x.CreatedByEmployee.EmployeeCode != null ? x.CreatedByEmployee.EmployeeCode : "Not Available"));

            //RFQVendor
            CreateMap<RFQVendor, RFQVendorDTOPost>().ReverseMap();
            CreateMap<RFQVendorDTOPost, RFQVendor>()
              .ForMember(dest => dest.RFQVendorId, opt => opt.Ignore());
            CreateMap<RFQVendor, RFQVendorDTOGet>()
                .ForMember(x => x.RFQTitle, x => x.MapFrom(x => x.RFQ.Title != null ? x.RFQ.Title : "Not Available"))
                .ForMember(x => x.VendorCode, x => x.MapFrom(x => x.Vendor.VendorCode != null ? x.Vendor.VendorCode : "Not Available"));

            //Quotation
            CreateMap<Quotation, QuotationDTOPost>().ReverseMap();
            CreateMap<QuotationDTOPost, Quotation>()
             .ForMember(dest => dest.QuotationId, opt => opt.Ignore());
            CreateMap<Quotation, QuotationDTOGet>()
                .ForMember(x => x.RFQTitle, x => x.MapFrom(x => x.RFQ.Title != null ? x.RFQ.Title : "Not Available"))
                .ForMember(x => x.VendorCode, x => x.MapFrom(x => x.Vendor.VendorCode != null ? x.Vendor.VendorCode : "Not Available"));

            //QuotationItem
            CreateMap<QuotationItem, QuotationItemDTOPost>().ReverseMap();
            CreateMap<QuotationItemDTOPost, QuotationItem>()
                .ForMember(dest => dest.QuotationItemId, opt => opt.Ignore());
            CreateMap<QuotationItem, QuotationItemDTOGet>()
                .ForMember(x => x.QuotationNumber, x => x.MapFrom(x => x.Quotation.QuotationNumber != null ? x.Quotation.QuotationNumber : "Not Available"))
                .ForMember(x => x.PRItemName, x => x.MapFrom(x => x.PurchaseRequisitionItem.ItemName != null ? x.PurchaseRequisitionItem.ItemName : "Not Available"));

            //VendorSelection
            CreateMap<VendorSelection, VendorSelectionDTOPost>().ReverseMap();
            CreateMap<VendorSelectionDTOPost, VendorSelection>()
                .ForMember(dest => dest.VendorSelectionId, opt => opt.Ignore());
            CreateMap<VendorSelection, VendorSelectionDTOGet>()
                .ForMember(x => x.RFQTitle, x => x.MapFrom(x => x.RFQ.Title != null ? x.RFQ.Title : "Not Available"))
                .ForMember(x => x.QuotationNumber, x => x.MapFrom(x => x.Quotation.QuotationNumber != null ? x.Quotation.QuotationNumber : "Not Available"))
                .ForMember(x => x.SelectedVendorCode, x => x.MapFrom(x => x.SelectedVendor.VendorCode != null ? x.SelectedVendor.VendorCode : "Not Available"))
                .ForMember(x => x.SelectedBy, x => x.MapFrom(x => x.SelectedByUser.FullName != null ? x.SelectedByUser.FullName : "Not Available"));

            //Approval Flow
            
            CreateMap<ApprovalFlow, ApprovalFlowDTOPost>().ReverseMap();
            CreateMap<ApprovalFlowDTOPost, ApprovalFlow>()
                .ForMember(dest => dest.ApprovalFlowId, opt => opt.Ignore());
            CreateMap<ApprovalFlow, ApprovalFlowDTOGet>()                
                .ForMember(x => x.RequiredRole, x => x.MapFrom(x => x.RequiredRole.RoleName != null ? x.RequiredRole.RoleName : "Not Available"));


        }
    }
}
