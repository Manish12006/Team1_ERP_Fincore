using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.DTO.MasterTable
{
    public class CurrencyResponseDto
    {
        public int CurrencyId { get; set; }

        public string CurrencyName { get; set; } 

        public string Symbol { get; set; } 
    }
}