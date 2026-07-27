using System.ComponentModel.DataAnnotations;

namespace Fincore.Application.DTO.Logs
{
    public class ApprovalLogDto
    {
        [Required]
        public string EntityName { get; set; }


        [Required]
        public long EntityId { get; set; }


        [Required]
        public int ApproverId { get; set; }


        [Required]
        [StringLength(100)]
        public string Status { get; set; }


        [StringLength(500)]
        public string Remarks { get; set; }


        [Required]
        public DateTime ActionDate { get; set; }
    }
}