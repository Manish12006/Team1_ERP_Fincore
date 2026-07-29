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
    public class RFQDTOPost
    {
        public int RFQId { get; set; }
        public string RFQNumber { get; set; }
        public int PurchaseRequisitionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? LastDate { get; set; }
        public byte? IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
