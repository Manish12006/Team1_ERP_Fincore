using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fincore.Domain.Models
{
    public class GRNItem
    {
        [Key]
        public int GRNItemId { get; set; }

        [Required]
        [ForeignKey("GRN")]
        public int GRNId { get; set; }
        public GRN GRN { get; set; }

        [Required]
        [ForeignKey("PurchaseOrderItem")]
        public int POItemId { get; set; }
        public PurchaseOrderItem PurchaseOrderItem { get; set; }

        [StringLength(200)]
        public string? Remarks { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Qty { get; set; }

        public List<Asset>? Assets { get; set; }
    }
}