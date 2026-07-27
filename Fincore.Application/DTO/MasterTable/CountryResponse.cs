namespace Fincore.Application.DTOs.MasterTable
{
    public class CountryResponseDto
    {
        public int CountryId { get; set; }
        public int CountryCode { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public int CurrencyId { get; set; }
        public int IsActive { get; set; }
        public DateTime CreatedAt { get; set; } 
        public DateTime? ModifiedAt { get; set; } 
    }
}