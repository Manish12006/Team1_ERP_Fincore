using System;
using System.ComponentModel.DataAnnotations;

namespace Fincore.Application.DTO.Capex
{
    public class PurchaseOrderDTO
    {

        public int POId { get; set; }


        [StringLength(30)]
        public string? POCode { get; set; }



        [Required(ErrorMessage = "Purchase Requisition is required")]
        [Range(1, int.MaxValue,
        ErrorMessage = "Purchase Requisition Id must be greater than 0")]
        public int PurchaseRequisitionId { get; set; }



        [Required(ErrorMessage = "Quotation is required")]
        [Range(1, int.MaxValue,
        ErrorMessage = "Quotation Id must be greater than 0")]
        public int QuotationId { get; set; }



        [Required(ErrorMessage = "Vendor is required")]
        [Range(1, int.MaxValue,
        ErrorMessage = "Vendor Id must be greater than 0")]
        public int VendorId { get; set; }



        // Response only
        public string? VendorCode { get; set; }



        [Required(ErrorMessage = "Requested By is required")]
        [Range(1, int.MaxValue,
        ErrorMessage = "Requested By must be greater than 0")]
        public int RequestedBy { get; set; }



        public DateTime? RequiredTillDate { get; set; }



        public DateTime? OrderDate { get; set; }



        public string? ApprovalStatus { get; set; }



        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

         

        public int? ApprovedBy { get; set; }



        public DateTime? ApprovedAt { get; set; }



        // Response only
        public byte IsActive { get; set; }



        public DateTime? CreatedAt { get; set; }



        public DateTime? ModifiedAt { get; set; }



        public int? CreatedBy { get; set; }



        public int? ModifiedBy { get; set; }


    }
}