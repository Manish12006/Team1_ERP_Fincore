using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Domain.Models
{
    public class PurchaseOrderItem
    {
        [Key]
        public int POItemId { get; set; }

        [Required]
        [ForeignKey("PurchaseOrder")]
        public int POId { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; }

        [Required]
        [ForeignKey("PurchaseRequisitionItem")]
        public int PRItemId { get; set; }
        public PurchaseRequisitionItem PurchaseRequisitionItem { get; set; }

        [Required]
        [StringLength(40)]
        public string ItemName { get; set; }

        [StringLength(500)]
        public string ItemDescription { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        [Required]
        [StringLength(20)]
        public string UnitOfMaterial { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal TaxPercentage { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        [Required]
        [StringLength(10)]
        public string ItemStatus { get; set; }
    }
}





//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace Fincore.Domain.Models
//{
//    public class PurchaseOrderItem
//    {
//        [Key]
//        public int POItemId { get; set; }

//        [Required]
//        [ForeignKey("PurchaseOrder")]
//        public int PurchaseOrderId { get; set; }
//        public PurchaseOrder PurchaseOrder { get; set; }

//        [Required]
//        [ForeignKey("QuotationItem")]
//        public int QuotationItemId { get; set; }
//        public QuotationItem QuotationItem { get; set; }

//        [Required]
//        [StringLength(100)]
//        public string ItemName { get; set; }

//        [Column(TypeName = "decimal(18,2)")]
//        public decimal UnitPrice { get; set; }

//        [Column(TypeName = "decimal(18,2)")]
//        public decimal Tax { get; set; }

//        [Column(TypeName = "decimal(18,2)")]
//        public decimal Discount { get; set; }

//        public List<GRNItem>? GRNItems { get; set; }
//    }
//}