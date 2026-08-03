using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Domain.Models
{
    public class GRN
    {
        [Key]
        public int GRNId { get; set; }

        [Required]
        [StringLength(30)]
        public string GRNCode { get; set; }

        [Required]
        [ForeignKey("PurchaseOrder")]
        public int POId { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; }

        [Required]
        [ForeignKey("Vendor")]
        public int VendorId { get; set; }
        public Vendor Vendor { get; set; }

        public byte? IsActive { get; set; }
        public DateTime? ReceivedDate { get; set; }

        [Required]
        [ForeignKey("ReceivedByUser")]
        public int ReceivedBy { get; set; }
        public User ReceivedByUser { get; set; }

        [Required]
        [StringLength(20)]
        public string QualityCheckStatus { get; set; }

        [ForeignKey("QualityCheckedByUser")]
        public int? QualityCheckedBy { get; set; }
        public User QualityCheckedByUser { get; set; }

        [Required]
        [StringLength(20)]
        public string GRNStatus { get; set; }

        [StringLength(500)]
        public string Remarks { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        [Required]
        [ForeignKey("CreatedByEmployee")]
        public int CreatedBy { get; set; }
        public Employee CreatedByEmployee { get; set; }

        // Navigation Properties
        public List<Asset> Assets { get; set; }
        public List<APInvoice> APInvoices { get; set; }
    }
}










//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace Fincore.Domain.Models
//{
//    public class GRN
//    {
//        [Key]
//        public int GRNId { get; set; }

//        [Required]
//        [ForeignKey("PurchaseOrder")]
//        public int PurchaseOrderId { get; set; }
//        public PurchaseOrder PurchaseOrder { get; set; }

//        [Required]
//        [StringLength(30)]
//        public string GRNNumber { get; set; }

//        [Required]
//        [ForeignKey("User")]
//        public int ReceivedBy { get; set; }
//        public User User { get; set; }

//        public DateTime? ReceivedDate { get; set; }

//        [StringLength(500)]
//        public string? Remarks { get; set; }

//        [StringLength(50)]
//        public string? DeliveryChallanNumber { get; set; }

//        [StringLength(20)]
//        public string Status { get; set; }

//        public List<GRNItem>? GRNItems { get; set; }
//    }
//}