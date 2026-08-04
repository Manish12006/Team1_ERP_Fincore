using Fincore.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.DTO.Capex.PurchaseOrder
{
    public class PMItemDTO
    {

        public int POId { get; set; }
        public string POCode { get; set; } = "";
        public int PurchaseRequisitionId { get; set; }
        public int QuotationId { get; set; }
        public int VendorId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Draft";
        public byte IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }

    }
}
