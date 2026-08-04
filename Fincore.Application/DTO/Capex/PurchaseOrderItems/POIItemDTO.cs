using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.DTO.Capex.PurchaseOrderItems
{
    public class POIItemDTO
    {
        public int POItemId { get; set; }
        public int POId { get; set; }
        public int PRItemId { get; set; }
        public int QuotationItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMaterial { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal LineTotal { get; set; }
        public string ItemStatus { get; set; }
    }
}
