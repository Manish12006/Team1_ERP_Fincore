using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.DTO.Capex
{
    public class PRDTOGet
    {
        public int PurchaseRequisitionId { get; set; }
        public string PRNumber { get; set; }
        public int? CapexRequestId { get; set; }
        public string PRTitle { get; set; }
        public string RequestedByName { get; set; }
        public DateTime? RequiredTillDate { get; set; }
        public string ApprovalStatus { get; set; }
        public string? ApprovedByName { get; set; }
        public byte? IsActive { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? ModifiedByName { get; set; }

    }
}
