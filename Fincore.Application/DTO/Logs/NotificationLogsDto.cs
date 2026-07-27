using System.ComponentModel.DataAnnotations;

namespace Fincore.Application.DTO.Logs
{
    public class NotificationLogDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public DateTime SentAt { get; set; }
    }
}