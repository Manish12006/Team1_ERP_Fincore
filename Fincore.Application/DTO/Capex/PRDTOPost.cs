using Fincore.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.DTO.Capex
{
    public class PRDTOPost
    {
        public int PurchaseRequisitionId { get; set; }
        public string PRNumber { get; set; }
        public int? CapexRequestId { get; set; }
        public string PRTitle { get; set; }
        public int RequestedBy { get; set; }
        public DateTime? RequiredTillDate { get; set; }
        public byte? IsActive { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }
}
