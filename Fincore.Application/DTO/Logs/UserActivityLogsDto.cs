using System.ComponentModel.DataAnnotations;

namespace Fincore.Application.DTO.Logs
{
    public class UserActivityLogDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string ActivityType { get; set; }

        [StringLength(100)]
        public string Module { get; set; }

        [Required]
        public DateTime ActivityDate { get; set; }
    }
}