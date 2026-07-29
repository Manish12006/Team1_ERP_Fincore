using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.DTO.Capex
{
    public class RFQDTOGet
    {
        public int RFQId { get; set; }
        public string RFQNumber { get; set; }
        public int PurchaseRequisitionId { get; set; }
        public string PRName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? LastDate { get; set; }
        public byte? IsActive { get; set; }
        public string CreatedByUser { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
