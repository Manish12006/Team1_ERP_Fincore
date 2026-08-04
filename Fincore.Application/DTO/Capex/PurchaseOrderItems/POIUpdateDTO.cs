using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.DTO.Capex.PurchaseOrderItems
{
    public class POIUpdateDTO : POICreateDTO
    {
        public int POItemId { get; set; }
    }
}
