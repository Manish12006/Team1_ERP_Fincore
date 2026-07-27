using System.ComponentModel.DataAnnotations;

namespace Fincore.Application.DTO.MasterTable
{
    public class CityDto
    {
        public string CityName { get; set; }

        public int StateId { get; set; }
    }
}