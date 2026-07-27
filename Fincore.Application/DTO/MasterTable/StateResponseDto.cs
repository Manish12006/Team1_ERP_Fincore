namespace Fincore.Application.DTOs.MasterTable
{
    public class StateResponseDto
    {
        public int StateId { get; set; }

        public string StateName { get; set; } = string.Empty;

        public int CountryId { get; set; }

        public string CountryName { get; set; } = string.Empty;
    }
}