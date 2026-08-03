using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Domain.Models
{
    public class QuotationItem
    {
        [Key]
        public int QuotationItemId { get; set; }

        [Required]
        [ForeignKey("Quotation")]
        public int QuotationId { get; set; }
        public Quotation Quotation { get; set; }

        [Required]
        [ForeignKey("PurchaseRequisitionItem")]
        public int PRItemId { get; set; }
        public PurchaseRequisitionItem PurchaseRequisitionItem { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TaxPercentage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }




        public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
    }

}
