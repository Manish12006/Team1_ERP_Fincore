using System.ComponentModel.DataAnnotations;

namespace Fincore.Application.DTOs.MasterTable
{
    public class StateDto
    {
        [Required]
        public string StateName { get; set; } = string.Empty;

        [Required]
        public int CountryId { get; set; }
    }
}