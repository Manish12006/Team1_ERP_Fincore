using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.DTO.Capex.PurchaseOrder
{
    public class PMUpdateDTO : PMCreateDTO
    {
        public int POId { get; set; }
        public int IsActive { get; set; }
    }
}
