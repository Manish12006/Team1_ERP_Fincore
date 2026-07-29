using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Domain.Models
{
    public class RFQ
    {
        [Key]
        public int RFQId { get; set; }

        [Required]
        [StringLength(30)]
        public string RFQNumber { get; set; }

        [Required]
        [ForeignKey("PurchaseRequisition")]
        public int PurchaseRequisitionId { get; set; }
        public PurchaseRequisition PurchaseRequisition { get; set; }

        [Required]
        [StringLength(30)]
        public string Title { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public DateTime? IssueDate { get; set; }
        public DateTime? LastDate { get; set; }

        public byte? IsActive { get; set; }

        [Required]
        [ForeignKey("CreatedByEmployee")]
        public int CreatedBy { get; set; }
        public Employee CreatedByEmployee { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        // Navigation Properties
        public List<RFQVendor> RFQVendors { get; set; }
        public List<Quotation> Quotations { get; set; }
        public List<VendorSelection> VendorSelections { get; set; }
    }
}
