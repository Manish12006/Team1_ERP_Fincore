namespace Fincore.Application.DTO.Logs
{
    public class UserActivityLogResponseDto
    {
        public long UserActivityLogId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string ActivityType { get; set; }

        public string Module { get; set; }

        public DateTime ActivityDate { get; set; }
    }
}