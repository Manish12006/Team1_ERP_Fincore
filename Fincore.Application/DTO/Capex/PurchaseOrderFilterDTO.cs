using Fincore.Domain.Enums;

namespace Fincore.Application.DTO.Capex
{
    public class PurchaseOrderFilterDTO
    {

        public string? POCode { get; set; }


        public ApprovalStatus? Status { get; set; }



        public int? PurchaseRequisitionId { get; set; }


        public int? QuotationId { get; set; }


        public int? VendorId { get; set; }


        public int? RequestedBy { get; set; }



        public DateTime? FromDate { get; set; }


        public DateTime? ToDate { get; set; }



        public decimal? MinAmount { get; set; }


        public decimal? MaxAmount { get; set; }



        public int? CreatedBy { get; set; }


        public int? ApprovedBy { get; set; }



        // Pagination

        public int Page { get; set; } = 1;


        public int PageSize { get; set; } = 10;

    }
}