using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Domain.Models
{
    public class PurchaseRequisitionItem
    {
        [Key]
        public int PRItemId { get; set; }

        [Required]
        [ForeignKey("PurchaseRequisition")]
        public int PurchaseRequisitionId { get; set; }
        public PurchaseRequisition PurchaseRequisition { get; set; }

        [Required]
        [StringLength(30)]
        public string ItemName { get; set; }

        [StringLength(500)]
        public string ItemDescription { get; set; }

        [Required]
        [ForeignKey("VendorCategory")]
        public int CategoryId { get; set; }
        public VendorCategory VendorCategory { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        [Required]
        [StringLength(20)]
        public string UnitOfMaterial { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedUnitPrice { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TaxPercentage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        [Required]
        [StringLength(10)]
        public string ItemStatus { get; set; }

        // Navigation Properties
        public List<QuotationItem> QuotationItems { get; set; }
        public List<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    }
}
