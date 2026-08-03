using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Domain.Models
{
    public class Asset
    {
        [Key]
        public int AssetId { get; set; }

        [Required]
        [StringLength(50)]
        public string AssetCode { get; set; }

        [Required]
        [StringLength(40)]
        public string AssetName { get; set; }

        [ForeignKey("CapexRequest")]
        public int? CapexRequestId { get; set; }
        public CapexRequest CapexRequest { get; set; }

        [ForeignKey("PurchaseOrder")]
        public int? PurchaseOrderId { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; }

        [ForeignKey("GRN")]
        public int? GRNId { get; set; }
        public GRN GRN { get; set; }

        [ForeignKey("Vendor")]
        public int? VendorId { get; set; }
        public Vendor Vendor { get; set; }

        [ForeignKey("Department")]
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        public DateTime? PurchaseDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PurchaseCost { get; set; }

        [StringLength(20)]
        public string Status { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}










//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace Fincore.Domain.Models
//{
//    public class Asset
//    {
//        [Key]
//        public int AssetId { get; set; }

//        [Required]
//        [ForeignKey("GRNItem")]
//        public int GRNItemId { get; set; }
//        public GRNItem GRNItem { get; set; }

//        [Required]
//        [ForeignKey("PurchaseOrderItem")]
//        public int POItemId { get; set; }
//        public PurchaseOrderItem PurchaseOrderItem { get; set; }

//        [Required]
//        [StringLength(50)]
//        public string AssetCode { get; set; }

//        [Required]
//        [StringLength(100)]
//        public string AssetName { get; set; }

//        [Column(TypeName = "decimal(18,2)")]
//        public decimal PurchaseCost { get; set; }

//        public DateTime? PurchaseDate { get; set; }

//        [StringLength(20)]
//        public string Status { get; set; }

//        [ForeignKey("User")]
//        public int? AssignedTo { get; set; }
//        public User? User { get; set; }
//    }
//}