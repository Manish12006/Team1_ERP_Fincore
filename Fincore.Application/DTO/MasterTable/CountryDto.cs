using System.ComponentModel.DataAnnotations;

namespace Fincore.Application.DTOs.MasterTable
{
    public class CountryDto
    {
        [Required]
        public int CountryCode { get; set; }

        [Required]
        [StringLength(30)]
        public string CountryName { get; set; } = string.Empty;

        [Required]
        public int CurrencyId { get; set; }
    }
}