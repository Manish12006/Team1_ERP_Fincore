using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.DTO.Capex
{
    public class QuotationDetailsDTO
    {
        public int QuotationId { get; set; }
        public string QuotationNumber { get; set; }

        public int? PurchaseRequisitionId { get; set; }

        public int VendorId { get; set; }
        public string VendorCode { get; set; }
    }
}
