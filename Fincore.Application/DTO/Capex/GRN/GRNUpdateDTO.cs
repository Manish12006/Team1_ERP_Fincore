using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.DTO.Capex.GRN
{
    public class GRNUpdateDTO : GRNCreateDTO
    {
        public int GRNId { get; set; }
        public byte IsActive { get; set; }
    }
}
