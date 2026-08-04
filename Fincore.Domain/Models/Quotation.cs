using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Domain.Models
{
    public class Quotation
    {
        [Key]
        public int QuotationId { get; set; }

        [Required]
        [ForeignKey("RFQ")]
        public int RFQId { get; set; }
        public RFQ RFQ { get; set; }

        [Required]
        [ForeignKey("Vendor")]
        public int VendorId { get; set; }
        public Vendor Vendor { get; set; }

        [Required]
        [StringLength(50)]
        public string QuotationNumber { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal QuotedAmount { get; set; }

        [StringLength(500)]
        public string Remarks { get; set; }

        public byte? IsActive { get; set; }

        [Required]
        public byte IsSelected { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        [ForeignKey("PurchaseRequisition")]
        public int? PurchaseRequisitionId { get; set; }
        public PurchaseRequisition PurchaseRequisition { get; set; }



        // Navigation Properties
        public List<QuotationItem> QuotationItems { get; set; }
        public List<VendorSelection> VendorSelections { get; set; }
        public List<PurchaseOrder> PurchaseOrders { get; set; }
    }
}
