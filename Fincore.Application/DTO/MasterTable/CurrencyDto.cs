using System.ComponentModel.DataAnnotations;

namespace Fincore.Application.DTO.MasterTable
{
    public class CurrencyDto
    {
        [Required]
        [StringLength(20)]
        public string CurrencyName { get; set; }

        [StringLength(5)]
        public string Symbol { get; set; }
    }
}