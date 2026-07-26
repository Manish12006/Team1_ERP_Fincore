using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fincore.Application.DTO.MasterTable
{
    public class AccountMasterPostDTO
    {
        [Required]
        public string AccountName { get; set; }

    
    }
}
