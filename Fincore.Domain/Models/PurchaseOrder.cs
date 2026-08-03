
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Domain.Models
{
    public class PurchaseOrder
    {
        [Key]
        public int POId { get; set; }

        [Required]
        [StringLength(30)]
        public string POCode { get; set; }

        [ForeignKey("PurchaseRequisition")]
        public int? PurchaseRequisitionId { get; set; }
        public PurchaseRequisition PurchaseRequisition { get; set; }

        [Required]
        [ForeignKey("Quotation")]
        public int QuotationId { get; set; }
        public Quotation Quotation { get; set; }

        [Required]
        [ForeignKey("Vendor")]
        public int VendorId { get; set; }
        public Vendor Vendor { get; set; }

        [ForeignKey("RequestedByUser")]
        public int? RequestedBy { get; set; }
        public User RequestedByUser { get; set; }

        public DateTime? RequiredTillDate { get; set; }
        public DateTime? OrderDate { get; set; }

        [Required]
        [StringLength(30)]
        public string ApprovalStatus { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [ForeignKey("ApprovedByUser")]
        public int? ApprovedBy { get; set; }
        public User ApprovedByUser { get; set; }

        public byte? IsActive { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? CreatedAt { get; set; }

        [Required]
        public DateTime ModifiedAt { get; set; }

        [Required]
        [ForeignKey("CreatedByUser")]
        public int CreatedBy { get; set; }
        public User CreatedByUser { get; set; }

        [Required]
        [ForeignKey("ModifiedByUser")]
        public int ModifiedBy { get; set; }
        public User ModifiedByUser { get; set; }

        // Navigation Properties
        public List<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public List<GRN> GRNs { get; set; }
        public List<Asset> Assets { get; set; }
        public List<APInvoice> APInvoices { get; set; }
    }
}











//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace Fincore.Domain.Models
//{
//    public class PurchaseOrder
//    {
//        [Key]
//        public int PurchaseOrderId { get; set; }

//        [Required]
//        [ForeignKey("Vendor")]
//        public int VendorId { get; set; }
//        public Vendor Vendor { get; set; }

//        [Required]
//        [ForeignKey("Quotation")]
//        public int QuotationId { get; set; }
//        public Quotation Quotation { get; set; }

//        [Required]
//        [StringLength(30)]
//        public string PONumber { get; set; }

//        [Column(TypeName = "decimal(18,2)")]
//        public decimal TotalAmount { get; set; }

//        public DateTime? PODate { get; set; }

//        [StringLength(20)]
//        public string Status { get; set; }

//        [StringLength(50)]
//        public string? DocumentNumber { get; set; }

//        public List<PurchaseOrderItem>? PurchaseOrderItems { get; set; }

//        public List<GRN>? GRNs { get; set; }
//    }
//}