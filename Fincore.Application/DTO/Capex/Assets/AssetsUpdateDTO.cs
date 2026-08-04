using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.DTO.Capex.Assets
{
    public class AssetsUpdateDTO : AssetsCreateDTO
    {
        public int AssetId { get; set; }
    }
}
