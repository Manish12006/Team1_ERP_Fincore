using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Domain.Models
{
    public class PurchaseRequisition
    {
        [Key]
        public int PurchaseRequisitionId { get; set; }

        [Required]
        [StringLength(50)]
        public string PRNumber { get; set; }

        [ForeignKey("CapexRequest")]
        public int? CapexRequestId { get; set; }
        public CapexRequest CapexRequest { get; set; }

        [Required]
        [StringLength(255)]
        public string PRTitle { get; set; }

        [Required]
        [ForeignKey("RequestedByUser")]
        public int RequestedBy { get; set; }
        public User RequestedByUser { get; set; }

        public DateTime? RequiredTillDate { get; set; }

        [Required]
        [StringLength(30)]
        public string ApprovalStatus { get; set; }

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
        public List<PurchaseRequisitionItem> PurchaseRequisitionItems { get; set; }
        public List<RFQ> RFQs { get; set; }
    }
}
